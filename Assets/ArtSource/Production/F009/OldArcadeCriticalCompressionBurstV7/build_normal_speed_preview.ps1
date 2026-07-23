$ErrorActionPreference = 'Stop'

$root = Split-Path -Parent $MyInvocation.MyCommand.Path
$node = 'C:\Users\admin\.cache\codex-runtimes\codex-primary-runtime\dependencies\node\bin\node.exe'
$nodeModules = 'C:\Users\admin\.cache\codex-runtimes\codex-primary-runtime\dependencies\node\node_modules'
$ffmpeg = 'C:\Users\admin\.codex\tools\wabish-motion-preview\node_modules\ffmpeg-static\ffmpeg.exe'
$fontFile = 'C\:/Windows/Fonts/msyh.ttc'
$frames = Join-Path $root '_frames_critical_normal'
$audio = Join-Path $root 'f009_v7_r1_critical_normal_speed_temp_audio_20260722.wav'
$video = Join-Path $root 'f009_v7_r1_critical_normal_speed_preview_20260722.mp4'
$vacuumFrame = Join-Path $root 'f009_v7_r1_critical_normal_speed_vacuum_20260722.png'
$peakFrame = Join-Path $root 'f009_v7_r1_critical_normal_speed_detonation_peak_20260722.png'
$resultFrame = Join-Path $root 'f009_v7_r1_critical_normal_speed_result_20260722.png'
$phaseBoard = Join-Path $root 'f009_v7_r1_critical_normal_speed_five_phase_review_board_20260722.png'
$timingComparison = Join-Path $root 'f009_v7_vs_v7_r1_timing_side_by_side_20260722.mp4'
$sequentialComparison = Join-Path $root 'f009_v7_vs_v7_r1_normal_speed_sequential_audio_comparison_20260722.mp4'
$v7Video = Join-Path $root 'f009_v7_critical_compression_burst_preview_20260722.mp4'

if (-not (Test-Path -LiteralPath $node)) { throw "Node runtime not found: $node" }
if (-not (Test-Path -LiteralPath $ffmpeg)) { throw "ffmpeg not found: $ffmpeg" }
if (-not (Test-Path -LiteralPath $v7Video)) { throw "V7 comparison video not found: $v7Video" }

$env:NODE_PATH = $nodeModules
New-Item -ItemType Directory -Path $frames -Force | Out-Null
Get-ChildItem -LiteralPath $frames -File | Where-Object {
  $_.Name -like 'frame_*.png' -or $_.Name -like 'frame_*.png.meta'
} | Remove-Item -Force

& $node (Join-Path $root 'build_audio.js') --tier critical --variant normal
if ($LASTEXITCODE -ne 0) { throw 'V7-R1 normal-speed audio build failed.' }

& $node (Join-Path $root 'render_preview.js') --tier critical --variant normal --fps 60
if ($LASTEXITCODE -ne 0) { throw 'V7-R1 normal-speed frame render failed.' }

& $ffmpeg -hide_banner -loglevel warning -y `
  -thread_queue_size 1024 -framerate 60 -start_number 0 -i (Join-Path $frames 'frame_%04d.png') `
  -i $audio -t 7.9 `
  -c:v libx264 -preset medium -crf 14 -profile:v high -pix_fmt yuv420p `
  -c:a aac -b:a 224k -movflags +faststart -shortest $video
if ($LASTEXITCODE -ne 0) { throw 'V7-R1 normal-speed MP4 encode failed.' }

$phaseSpecs = @(
  [PSCustomObject]@{ Frame = '0345'; Label = '01 锁死 · 1.0x' },
  [PSCustomObject]@{ Frame = '0371'; Label = '02 吸光 · 1.0x' },
  [PSCustomObject]@{ Frame = '0401'; Label = '03 真空 · 1.0x' },
  [PSCustomObject]@{ Frame = '0412'; Label = '04 击穿 · 1.0x' },
  [PSCustomObject]@{ Frame = '0427'; Label = '05 余震 · 1.0x' }
)
$phasePaths = @()
foreach ($phase in $phaseSpecs) {
  $phasePaths += Join-Path $frames ("frame_" + $phase.Frame + '.png')
}
Copy-Item -LiteralPath (Join-Path $frames 'frame_0401.png') -Destination $vacuumFrame -Force
Copy-Item -LiteralPath (Join-Path $frames 'frame_0412.png') -Destination $peakFrame -Force
Copy-Item -LiteralPath (Join-Path $frames 'frame_0445.png') -Destination $resultFrame -Force

$phaseFilters = @()
for ($i = 0; $i -lt $phaseSpecs.Count; $i++) {
  $phaseFilters += "[$i`:v]scale=640:360,pad=640:396:0:36:color=0x080705,drawtext=fontfile='$fontFile':text='$($phaseSpecs[$i].Label)':fontcolor=0xF1A02D:fontsize=24:x=18:y=5[p$i]"
}
$phaseGraph = ($phaseFilters -join ';') + ';[p0][p1][p2][p3][p4]xstack=inputs=5:layout=0_0|640_0|1280_0|320_396|960_396:fill=0x080705[stack];[stack]pad=1920:1080:0:144:color=0x080705[outv]'
& $ffmpeg -hide_banner -loglevel error -y `
  -i $phasePaths[0] -i $phasePaths[1] -i $phasePaths[2] -i $phasePaths[3] -i $phasePaths[4] `
  -filter_complex $phaseGraph -map '[outv]' -frames:v 1 -update 1 $phaseBoard
if ($LASTEXITCODE -ne 0) { throw 'V7-R1 normal-speed five-phase review board build failed.' }

$timingGraph = "[0:v]tpad=stop_mode=clone:stop_duration=2.7,scale=640:360,pad=640:404:0:44:color=0x080705,drawtext=fontfile='$fontFile':text='V7 样片节拍 · 约 1.8x':fontcolor=0xB87332:fontsize=25:x=18:y=7[fast];[1:v]scale=640:360,pad=640:404:0:44:color=0x080705,drawtext=fontfile='$fontFile':text='V7-R1 游戏正常速 · 1.0x':fontcolor=0xFFD278:fontsize=25:x=18:y=7[normal];[fast][normal]hstack=inputs=2[outv]"
& $ffmpeg -hide_banner -loglevel warning -y `
  -i $v7Video -i $video -filter_complex $timingGraph -map '[outv]' -an -t 7.9 -r 60 `
  -c:v libx264 -preset medium -crf 15 -profile:v high -pix_fmt yuv420p -movflags +faststart $timingComparison
if ($LASTEXITCODE -ne 0) { throw 'V7/V7-R1 timing comparison encode failed.' }

$sequenceGraph = "[0:v]pad=1280:780:0:60:color=0x080705,drawtext=fontfile='$fontFile':text='V7 样片节拍 · 约 1.8x · 5.2 秒':fontcolor=0xB87332:fontsize=28:x=22:y=10[fast];[1:v]pad=1280:780:0:60:color=0x080705,drawtext=fontfile='$fontFile':text='V7-R1 游戏正常速 · 1.0x · 7.9 秒':fontcolor=0xFFD278:fontsize=28:x=22:y=10[normal];[fast][0:a][normal][1:a]concat=n=2:v=1:a=1[outv][outa]"
& $ffmpeg -hide_banner -loglevel warning -y `
  -i $v7Video -i $video -filter_complex $sequenceGraph -map '[outv]' -map '[outa]' `
  -c:v libx264 -preset medium -crf 15 -profile:v high -pix_fmt yuv420p `
  -c:a aac -b:a 224k -movflags +faststart $sequentialComparison
if ($LASTEXITCODE -ne 0) { throw 'V7/V7-R1 sequential audio comparison encode failed.' }

foreach ($item in @($video, $timingComparison, $sequentialComparison)) {
  & $ffmpeg -hide_banner -loglevel error -i $item -f null NUL
  if ($LASTEXITCODE -ne 0) { throw "Decoded MP4 verification failed: $item" }
}

$resolvedFrames = (Resolve-Path -LiteralPath $frames).Path
$expectedFrames = [System.IO.Path]::GetFullPath((Join-Path $root '_frames_critical_normal'))
if ($resolvedFrames -ne $expectedFrames) { throw "Unexpected frame cleanup target: $resolvedFrames" }
Get-ChildItem -LiteralPath $resolvedFrames -File | Where-Object {
  $_.Name -like 'frame_*.png' -or $_.Name -like 'frame_*.png.meta'
} | Remove-Item -Force
if ((Get-ChildItem -LiteralPath $resolvedFrames -Force | Measure-Object).Count -eq 0) {
  Remove-Item -LiteralPath $resolvedFrames -Force
  $framesMeta = $expectedFrames + '.meta'
  if (Test-Path -LiteralPath $framesMeta) { Remove-Item -LiteralPath $framesMeta -Force }
}

$outputs = @($video, $timingComparison, $sequentialComparison, $vacuumFrame, $peakFrame, $resultFrame, $phaseBoard)
foreach ($output in $outputs) {
  $item = Get-Item -LiteralPath $output
  $hash = (Get-FileHash -LiteralPath $output -Algorithm SHA256).Hash
  Write-Output ("OUTPUT={0}|BYTES={1}|SHA256={2}" -f $item.FullName, $item.Length, $hash)
}
Write-Output 'SPEC=V7-R1 1.0x simulation 1280x720 7.9s 60fps H.264/AAC; timing 1280x404; sequential 1280x780'
