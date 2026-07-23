$ErrorActionPreference = 'Stop'

$root = Split-Path -Parent $MyInvocation.MyCommand.Path
$node = 'C:\Users\admin\.cache\codex-runtimes\codex-primary-runtime\dependencies\node\bin\node.exe'
$nodeModules = 'C:\Users\admin\.cache\codex-runtimes\codex-primary-runtime\dependencies\node\node_modules'
$ffmpeg = 'C:\Users\admin\.codex\tools\wabish-motion-preview\node_modules\ffmpeg-static\ffmpeg.exe'
$fontFile = 'C\:/Windows/Fonts/msyh.ttc'
$tiers = @(
  [PSCustomObject]@{ Key = 'miss'; Slug = '01_miss'; Label = '01 未达到'; PosterFrame = '0229'; ResultFrame = '0264' },
  [PSCustomObject]@{ Key = 'pass'; Slug = '02_pass'; Label = '02 过关'; PosterFrame = '0229'; ResultFrame = '0264' },
  [PSCustomObject]@{ Key = 'exceed'; Slug = '03_exceed'; Label = '03 超过'; PosterFrame = '0229'; ResultFrame = '0264' },
  [PSCustomObject]@{ Key = 'far'; Slug = '04_far_exceed'; Label = '04 远超过'; PosterFrame = '0229'; ResultFrame = '0264' },
  [PSCustomObject]@{ Key = 'critical'; Slug = '05_critical'; Label = '05 暴击'; PosterFrame = '0229'; ResultFrame = '0264' }
)

if (-not (Test-Path -LiteralPath $node)) { throw "Node runtime not found: $node" }
if (-not (Test-Path -LiteralPath $ffmpeg)) { throw "ffmpeg not found: $ffmpeg" }

$env:NODE_PATH = $nodeModules
$videoPaths = @()
$posterPaths = @()
$resultPaths = @()

foreach ($tier in $tiers) {
  $frames = Join-Path $root ("_frames_" + $tier.Key)
  $audio = Join-Path $root ("f009_v6b_r6_tier_" + $tier.Slug + "_temp_audio_20260722.wav")
  $video = Join-Path $root ("f009_v6b_r6_tier_" + $tier.Slug + "_preview_20260722.mp4")
  $poster = Join-Path $root ("f009_v6b_r6_tier_" + $tier.Slug + "_peak_20260722.png")
  $result = Join-Path $root ("f009_v6b_r6_tier_" + $tier.Slug + "_result_20260722.png")

  New-Item -ItemType Directory -Path $frames -Force | Out-Null
  Get-ChildItem -LiteralPath $frames -Filter 'frame_*.png' -File | Remove-Item -Force

  & $node (Join-Path $root 'build_audio.js') --tier $tier.Key
  if ($LASTEXITCODE -ne 0) { throw "Audio build failed: $($tier.Key)" }

  & $node (Join-Path $root 'render_preview.js') --tier $tier.Key --fps 60
  if ($LASTEXITCODE -ne 0) { throw "Frame render failed: $($tier.Key)" }

  & $ffmpeg -hide_banner -loglevel warning -y `
    -thread_queue_size 1024 -framerate 60 -start_number 0 -i (Join-Path $frames 'frame_%04d.png') `
    -i $audio -t 4.8 `
    -c:v libx264 -preset medium -crf 15 -profile:v high -pix_fmt yuv420p `
    -c:a aac -b:a 192k -movflags +faststart -shortest $video
  if ($LASTEXITCODE -ne 0) { throw "MP4 encode failed: $($tier.Key)" }

  Copy-Item -LiteralPath (Join-Path $frames ("frame_" + $tier.PosterFrame + ".png")) -Destination $poster -Force
  Copy-Item -LiteralPath (Join-Path $frames ("frame_" + $tier.ResultFrame + ".png")) -Destination $result -Force

  & $ffmpeg -hide_banner -loglevel error -i $video -f null NUL
  if ($LASTEXITCODE -ne 0) { throw "Decoded MP4 verification failed: $($tier.Key)" }

  $resolvedFrames = (Resolve-Path -LiteralPath $frames).Path
  $expectedFrames = [System.IO.Path]::GetFullPath((Join-Path $root ("_frames_" + $tier.Key)))
  if ($resolvedFrames -ne $expectedFrames) { throw "Unexpected frame cleanup target: $resolvedFrames" }
  Get-ChildItem -LiteralPath $resolvedFrames -Filter 'frame_*.png' -File | Remove-Item -Force

  $videoPaths += $video
  $posterPaths += $poster
  $resultPaths += $result
}

$syncVideo = Join-Path $root 'f009_v6b_r6_five_tier_sync_visual_comparison_20260722.mp4'
$sequentialVideo = Join-Path $root 'f009_v6b_r6_five_tier_sequential_audio_comparison_20260722.mp4'
$peakBoard = Join-Path $root 'f009_v6b_r6_five_tier_peak_review_board_20260722.png'
$resultBoard = Join-Path $root 'f009_v6b_r6_five_tier_result_review_board_20260722.png'
$circuitBoard = Join-Path $root 'f009_v6b_r6_five_tier_circuit_detail_review_20260722.png'
$criticalSequenceBoard = Join-Path $root 'f009_v6b_r6_critical_brownout_sequence_review_20260722.png'

$panelFilters = @()
for ($i = 0; $i -lt $tiers.Count; $i++) {
  $panelFilters += "[$i`:v]scale=640:360,pad=640:396:0:36:color=0x080705,drawtext=fontfile='$fontFile':text='$($tiers[$i].Label)':fontcolor=0xF1A02D:fontsize=24:x=18:y=5[p$i]"
}
$syncFilter = ($panelFilters -join ';') + ';[p0][p1][p2][p3][p4]xstack=inputs=5:layout=0_0|640_0|1280_0|320_396|960_396:fill=0x080705[stack];[stack]pad=1920:1080:0:144:color=0x080705[outv]'
& $ffmpeg -hide_banner -loglevel warning -y `
  -i $videoPaths[0] -i $videoPaths[1] -i $videoPaths[2] -i $videoPaths[3] -i $videoPaths[4] `
  -filter_complex $syncFilter -map '[outv]' -an -r 60 `
  -c:v libx264 -preset medium -crf 16 -profile:v high -pix_fmt yuv420p -movflags +faststart $syncVideo
if ($LASTEXITCODE -ne 0) { throw 'Synchronized five-tier comparison encode failed.' }

$sequenceFilters = @()
for ($i = 0; $i -lt $tiers.Count; $i++) {
  $sequenceFilters += "[$i`:v]pad=1280:780:0:60:color=0x080705,drawtext=fontfile='$fontFile':text='$($tiers[$i].Label) · 评审标注':fontcolor=0xF1A02D:fontsize=28:x=22:y=10[s$i]"
}
$sequenceConcat = '[s0][0:a][s1][1:a][s2][2:a][s3][3:a][s4][4:a]concat=n=5:v=1:a=1[outv][outa]'
$sequenceFilter = ($sequenceFilters -join ';') + ';' + $sequenceConcat
& $ffmpeg -hide_banner -loglevel warning -y `
  -i $videoPaths[0] -i $videoPaths[1] -i $videoPaths[2] -i $videoPaths[3] -i $videoPaths[4] `
  -filter_complex $sequenceFilter -map '[outv]' -map '[outa]' `
  -c:v libx264 -preset medium -crf 16 -profile:v high -pix_fmt yuv420p `
  -c:a aac -b:a 192k -movflags +faststart $sequentialVideo
if ($LASTEXITCODE -ne 0) { throw 'Sequential five-tier comparison encode failed.' }

function Build-FullFrameBoard {
  param([string[]]$Inputs, [string]$Output)
  $filters = @()
  for ($i = 0; $i -lt $tiers.Count; $i++) {
    $filters += "[$i`:v]scale=640:360,pad=640:396:0:36:color=0x080705,drawtext=fontfile='$fontFile':text='$($tiers[$i].Label)':fontcolor=0xF1A02D:fontsize=24:x=18:y=5[p$i]"
  }
  $graph = ($filters -join ';') + ';[p0][p1][p2][p3][p4]xstack=inputs=5:layout=0_0|640_0|1280_0|320_396|960_396:fill=0x080705[stack];[stack]pad=1920:1080:0:144:color=0x080705[outv]'
  & $ffmpeg -hide_banner -loglevel error -y `
    -i $Inputs[0] -i $Inputs[1] -i $Inputs[2] -i $Inputs[3] -i $Inputs[4] `
    -filter_complex $graph -map '[outv]' -frames:v 1 -update 1 $Output
  if ($LASTEXITCODE -ne 0) { throw "Review board build failed: $Output" }
}

Build-FullFrameBoard -Inputs $posterPaths -Output $peakBoard
Build-FullFrameBoard -Inputs $resultPaths -Output $resultBoard

$circuitFilters = @()
for ($i = 0; $i -lt $tiers.Count; $i++) {
  $circuitFilters += "[$i`:v]crop=1060:560:110:100,scale=380:201,pad=384:243:2:40:color=0x080705,drawtext=fontfile='$fontFile':text='$($tiers[$i].Label)':fontcolor=0xF1A02D:fontsize=23:x=14:y=7[t$i]"
}
$circuitGraph = ($circuitFilters -join ';') + ';[t0][t1][t2][t3][t4]hstack=inputs=5[outv]'
& $ffmpeg -hide_banner -loglevel error -y `
  -i $posterPaths[0] -i $posterPaths[1] -i $posterPaths[2] -i $posterPaths[3] -i $posterPaths[4] `
  -filter_complex $circuitGraph -map '[outv]' -frames:v 1 -update 1 $circuitBoard
if ($LASTEXITCODE -ne 0) { throw 'Circuit detail review board build failed.' }

$criticalSequenceGraph = @(
  "[0:v]scale=640:360,pad=640:404:0:44:color=0x080705,drawtext=fontfile='$fontFile':text='3.60 负载顶点':fontcolor=0xF1A02D:fontsize=24:x=18:y=7[q0]",
  "[1:v]scale=640:360,pad=640:404:0:44:color=0x080705,drawtext=fontfile='$fontFile':text='3.70 全机欠压':fontcolor=0xF1A02D:fontsize=24:x=18:y=7[q1]",
  "[2:v]scale=640:360,pad=640:404:0:44:color=0x080705,drawtext=fontfile='$fontFile':text='3.82 二段复电':fontcolor=0xF1A02D:fontsize=24:x=18:y=7[q2]",
  "[3:v]scale=640:360,pad=640:404:0:44:color=0x080705,drawtext=fontfile='$fontFile':text='4.05 余辉冷却':fontcolor=0xF1A02D:fontsize=24:x=18:y=7[q3]",
  '[q0][q1][q2][q3]xstack=inputs=4:layout=0_0|640_0|0_404|640_404:fill=0x080705[outv]'
) -join ';'
& $ffmpeg -hide_banner -loglevel error -y `
  -ss 3.60 -i $videoPaths[4] -ss 3.70 -i $videoPaths[4] -ss 3.82 -i $videoPaths[4] -ss 4.05 -i $videoPaths[4] `
  -filter_complex $criticalSequenceGraph -map '[outv]' -frames:v 1 -update 1 $criticalSequenceBoard
if ($LASTEXITCODE -ne 0) { throw 'Critical brownout sequence review board build failed.' }

& $ffmpeg -hide_banner -loglevel error -i $syncVideo -f null NUL
if ($LASTEXITCODE -ne 0) { throw 'Synchronized comparison decode failed.' }
& $ffmpeg -hide_banner -loglevel error -i $sequentialVideo -f null NUL
if ($LASTEXITCODE -ne 0) { throw 'Sequential comparison decode failed.' }

$outputs = @($syncVideo, $sequentialVideo, $peakBoard, $resultBoard, $circuitBoard, $criticalSequenceBoard) + $videoPaths
foreach ($output in $outputs) {
  $item = Get-Item -LiteralPath $output
  $hash = (Get-FileHash -LiteralPath $output -Algorithm SHA256).Hash
  Write-Output ("OUTPUT={0}|BYTES={1}|SHA256={2}" -f $item.FullName, $item.Length, $hash)
}
Write-Output 'SPEC=individual 1280x720 / sync 1920x1080 / sequential 1280x780 / 60fps / H.264'
