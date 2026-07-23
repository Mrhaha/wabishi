$ErrorActionPreference = 'Stop'

$root = Split-Path -Parent $MyInvocation.MyCommand.Path
$node = 'C:\Users\admin\.cache\codex-runtimes\codex-primary-runtime\dependencies\node\bin\node.exe'
$nodeModules = 'C:\Users\admin\.cache\codex-runtimes\codex-primary-runtime\dependencies\node\node_modules'
$ffmpeg = 'C:\Users\admin\.codex\tools\wabish-motion-preview\node_modules\ffmpeg-static\ffmpeg.exe'
$fontFile = 'C\:/Windows/Fonts/msyh.ttc'
$criticalVideo = Join-Path $root 'f009_v7_r1_critical_normal_speed_preview_20260722.mp4'
$criticalSignature = Join-Path $root 'f009_v7_r1_critical_normal_speed_detonation_peak_20260722.png'
$criticalResult = Join-Path $root 'f009_v7_r1_critical_normal_speed_result_20260722.png'
$signatureBoard = Join-Path $root 'f009_v7_r2_five_tier_normal_speed_signature_board_20260722.png'
$resultBoard = Join-Path $root 'f009_v7_r2_five_tier_normal_speed_result_board_20260722.png'
$syncComparison = Join-Path $root 'f009_v7_r2_five_tier_normal_speed_sync_visual_comparison_20260722.mp4'
$sequentialComparison = Join-Path $root 'f009_v7_r2_five_tier_normal_speed_sequential_audio_comparison_20260722.mp4'

if (-not (Test-Path -LiteralPath $node)) { throw "Node runtime not found: $node" }
if (-not (Test-Path -LiteralPath $ffmpeg)) { throw "ffmpeg not found: $ffmpeg" }
foreach ($required in @($criticalVideo, $criticalSignature, $criticalResult)) {
  if (-not (Test-Path -LiteralPath $required)) { throw "Accepted V7-R1 reference not found: $required" }
}

$env:NODE_PATH = $nodeModules
$tiers = @(
  [PSCustomObject]@{ Key = 'miss'; Slug = '01_miss'; Label = '01 未达到 · 反向泄压'; SignatureFrame = '0352' },
  [PSCustomObject]@{ Key = 'pass'; Slug = '02_pass'; Label = '02 过关 · 单拍闭环'; SignatureFrame = '0366' },
  [PSCustomObject]@{ Key = 'exceed'; Slug = '03_exceed'; Label = '03 超过 · 双拍继电'; SignatureFrame = '0370' },
  [PSCustomObject]@{ Key = 'far'; Slug = '04_far_exceed'; Label = '04 远超过 · 整机过载'; SignatureFrame = '0374' }
)

$videos = @()
$signatureFrames = @()
$resultFrames = @()
$frameDirectories = @()

foreach ($tier in $tiers) {
  $frames = Join-Path $root ("_frames_" + $tier.Key + '_normal')
  $audio = Join-Path $root ("f009_v7_r2_tier_" + $tier.Slug + '_normal_speed_temp_audio_20260722.wav')
  $video = Join-Path $root ("f009_v7_r2_tier_" + $tier.Slug + '_normal_speed_preview_20260722.mp4')
  $signature = Join-Path $root ("f009_v7_r2_tier_" + $tier.Slug + '_normal_speed_signature_20260722.png')
  $result = Join-Path $root ("f009_v7_r2_tier_" + $tier.Slug + '_normal_speed_result_20260722.png')

  New-Item -ItemType Directory -Path $frames -Force | Out-Null
  Get-ChildItem -LiteralPath $frames -File | Where-Object {
    $_.Name -like 'frame_*.png' -or $_.Name -like 'frame_*.png.meta'
  } | Remove-Item -Force

  & $node (Join-Path $root 'build_audio.js') --tier $tier.Key --variant normal
  if ($LASTEXITCODE -ne 0) { throw "Audio build failed: $($tier.Key)" }

  & $node (Join-Path $root 'render_preview.js') --tier $tier.Key --variant normal --fps 60
  if ($LASTEXITCODE -ne 0) { throw "Frame render failed: $($tier.Key)" }

  & $ffmpeg -hide_banner -loglevel warning -y `
    -thread_queue_size 1024 -framerate 60 -start_number 0 -i (Join-Path $frames 'frame_%04d.png') `
    -i $audio -t 7.9 `
    -c:v libx264 -preset medium -crf 14 -profile:v high -pix_fmt yuv420p `
    -c:a aac -b:a 224k -movflags +faststart -shortest $video
  if ($LASTEXITCODE -ne 0) { throw "MP4 encode failed: $($tier.Key)" }

  Copy-Item -LiteralPath (Join-Path $frames ("frame_" + $tier.SignatureFrame + '.png')) -Destination $signature -Force
  Copy-Item -LiteralPath (Join-Path $frames 'frame_0427.png') -Destination $result -Force

  $videos += $video
  $signatureFrames += $signature
  $resultFrames += $result
  $frameDirectories += $frames
}

$videos += $criticalVideo
$signatureFrames += $criticalSignature
$resultFrames += $criticalResult
$labels = @(
  '01 未达到 · 反向泄压',
  '02 过关 · 单拍闭环',
  '03 超过 · 双拍继电',
  '04 远超过 · 整机过载',
  '05 最高暴击 · 真空击穿'
)
$colors = @('0xB5683D', '0xE6A447', '0xF1BA54', '0xFFD278', '0xFFF0B0')

function Build-FivePanelBoard([string[]]$inputs, [string]$output) {
  $filters = @()
  for ($i = 0; $i -lt 5; $i++) {
    $filters += "[$i`:v]scale=640:360,pad=640:400:0:40:color=0x080705,drawtext=fontfile='$fontFile':text='$($labels[$i])':fontcolor=$($colors[$i]):fontsize=24:x=18:y=6[p$i]"
  }
  $graph = ($filters -join ';') + ';[p0][p1][p2][p3][p4]xstack=inputs=5:layout=0_0|640_0|1280_0|320_400|960_400:fill=0x080705[outv]'
  & $ffmpeg -hide_banner -loglevel error -y `
    -i $inputs[0] -i $inputs[1] -i $inputs[2] -i $inputs[3] -i $inputs[4] `
    -filter_complex $graph -map '[outv]' -frames:v 1 -update 1 $output
  if ($LASTEXITCODE -ne 0) { throw "Five-panel board build failed: $output" }
}

Build-FivePanelBoard $signatureFrames $signatureBoard
Build-FivePanelBoard $resultFrames $resultBoard

$syncFilters = @()
for ($i = 0; $i -lt 5; $i++) {
  $syncFilters += "[$i`:v]scale=640:360,pad=640:400:0:40:color=0x080705,drawtext=fontfile='$fontFile':text='$($labels[$i])':fontcolor=$($colors[$i]):fontsize=24:x=18:y=6[p$i]"
}
$syncGraph = ($syncFilters -join ';') + ';[p0][p1][p2][p3][p4]xstack=inputs=5:layout=0_0|640_0|1280_0|320_400|960_400:fill=0x080705[outv]'
& $ffmpeg -hide_banner -loglevel warning -y `
  -i $videos[0] -i $videos[1] -i $videos[2] -i $videos[3] -i $videos[4] `
  -filter_complex $syncGraph -map '[outv]' -an -t 7.9 -r 60 `
  -c:v libx264 -preset medium -crf 15 -profile:v high -pix_fmt yuv420p -movflags +faststart $syncComparison
if ($LASTEXITCODE -ne 0) { throw 'Five-tier synchronized visual comparison encode failed.' }

$sequenceParts = @()
for ($i = 0; $i -lt 5; $i++) {
  $sequenceParts += "[$i`:v]pad=1280:780:0:60:color=0x080705,drawtext=fontfile='$fontFile':text='$($labels[$i]) · 游戏 1.0x':fontcolor=$($colors[$i]):fontsize=28:x=22:y=10[v$i]"
}
$sequenceInputs = (0..4 | ForEach-Object { "[v$_][$_`:a]" }) -join ''
$sequenceGraph = ($sequenceParts -join ';') + ";$sequenceInputs" + 'concat=n=5:v=1:a=1[outv][outa]'
& $ffmpeg -hide_banner -loglevel warning -y `
  -i $videos[0] -i $videos[1] -i $videos[2] -i $videos[3] -i $videos[4] `
  -filter_complex $sequenceGraph -map '[outv]' -map '[outa]' `
  -c:v libx264 -preset medium -crf 15 -profile:v high -pix_fmt yuv420p `
  -c:a aac -b:a 224k -movflags +faststart $sequentialComparison
if ($LASTEXITCODE -ne 0) { throw 'Five-tier sequential audio comparison encode failed.' }

foreach ($item in @($videos + @($syncComparison, $sequentialComparison))) {
  & $ffmpeg -hide_banner -loglevel error -i $item -f null NUL
  if ($LASTEXITCODE -ne 0) { throw "Decoded MP4 verification failed: $item" }
}

foreach ($frames in $frameDirectories) {
  $resolvedFrames = (Resolve-Path -LiteralPath $frames).Path
  $expectedFrames = [System.IO.Path]::GetFullPath($frames)
  if ($resolvedFrames -ne $expectedFrames -or -not $resolvedFrames.StartsWith([System.IO.Path]::GetFullPath($root) + [System.IO.Path]::DirectorySeparatorChar)) {
    throw "Unexpected frame cleanup target: $resolvedFrames"
  }
  Get-ChildItem -LiteralPath $resolvedFrames -File | Where-Object {
    $_.Name -like 'frame_*.png' -or $_.Name -like 'frame_*.png.meta'
  } | Remove-Item -Force
  if ((Get-ChildItem -LiteralPath $resolvedFrames -Force | Measure-Object).Count -eq 0) {
    Remove-Item -LiteralPath $resolvedFrames -Force
    $framesMeta = $expectedFrames + '.meta'
    if (Test-Path -LiteralPath $framesMeta) { Remove-Item -LiteralPath $framesMeta -Force }
  }
}

$outputs = @($videos[0..3] + $signatureFrames[0..3] + $resultFrames[0..3] + @($signatureBoard, $resultBoard, $syncComparison, $sequentialComparison))
foreach ($output in $outputs) {
  $item = Get-Item -LiteralPath $output
  $hash = (Get-FileHash -LiteralPath $output -Algorithm SHA256).Hash
  Write-Output ("OUTPUT={0}|BYTES={1}|SHA256={2}" -f $item.FullName, $item.Length, $hash)
}
Write-Output 'SPEC=V7-R2 tiers 01-04 + accepted V7-R1 tier 05; 1280x720 7.9s 60fps H.264/AAC; sync 1920x800; sequential 1280x780'
