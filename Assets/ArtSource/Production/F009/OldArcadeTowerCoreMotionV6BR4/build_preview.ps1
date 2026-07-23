$ErrorActionPreference = 'Stop'

$root = Split-Path -Parent $MyInvocation.MyCommand.Path
$node = 'C:\Users\admin\.cache\codex-runtimes\codex-primary-runtime\dependencies\node\bin\node.exe'
$nodeModules = 'C:\Users\admin\.cache\codex-runtimes\codex-primary-runtime\dependencies\node\node_modules'
$ffmpeg = 'C:\Users\admin\.codex\tools\wabish-motion-preview\node_modules\ffmpeg-static\ffmpeg.exe'
$frames = Join-Path $root '_frames'
$audio = Join-Path $root 'f009_v6b_r4_direct_glass_absorption_critical_temp_audio_20260722.wav'
$video = Join-Path $root 'f009_v6b_r4_direct_glass_absorption_critical_preview_20260722.mp4'
$review = Join-Path $root 'f009_v6b_r4_direct_glass_absorption_critical_review_20260722.png'
$detailReview = Join-Path $root 'f009_v6b_r4_direct_absorption_review_20260722.png'
$poster = Join-Path $root 'f009_v6b_r4_direct_glass_absorption_critical_poster_20260722.png'

if (-not (Test-Path -LiteralPath $node)) { throw "Node runtime not found: $node" }
if (-not (Test-Path -LiteralPath $ffmpeg)) { throw "ffmpeg not found: $ffmpeg" }
New-Item -ItemType Directory -Path $frames -Force | Out-Null
Get-ChildItem -LiteralPath $frames -Filter 'frame_*.png' -File | Remove-Item -Force

$env:NODE_PATH = $nodeModules
& $node (Join-Path $root 'build_audio.js')
if ($LASTEXITCODE -ne 0) { throw 'Audio build failed.' }

& $node (Join-Path $root 'render_preview.js') --fps 60
if ($LASTEXITCODE -ne 0) { throw 'Frame render failed.' }

& $ffmpeg -hide_banner -loglevel warning -y `
  -thread_queue_size 1024 -framerate 60 -start_number 0 -i (Join-Path $frames 'frame_%04d.png') `
  -i $audio -t 4.8 `
  -c:v libx264 -preset medium -crf 15 -profile:v high -pix_fmt yuv420p `
  -c:a aac -b:a 192k -movflags +faststart -shortest $video
if ($LASTEXITCODE -ne 0) { throw 'MP4 encode failed.' }

$select = "select='eq(n,0)+eq(n,18)+eq(n,51)+eq(n,72)+eq(n,91)+eq(n,114)+eq(n,132)+eq(n,167)+eq(n,186)+eq(n,219)+eq(n,229)+eq(n,270)',scale=320:180,tile=4x3:padding=4:margin=4:color=0x080705"
& $ffmpeg -hide_banner -loglevel warning -y `
  -framerate 60 -start_number 0 -i (Join-Path $frames 'frame_%04d.png') `
  -vf $select -fps_mode vfr -frames:v 1 -update 1 $review
if ($LASTEXITCODE -ne 0) { throw 'Review sheet build failed.' }

$detailSelect = "select='eq(n,97)+eq(n,100)+eq(n,102)+eq(n,104)+eq(n,107)+eq(n,115)+eq(n,117)+eq(n,119)+eq(n,122)+eq(n,130)+eq(n,132)+eq(n,134)+eq(n,205)+eq(n,220)+eq(n,229)',scale=256:144,tile=5x3:padding=4:margin=4:color=0x080705"
& $ffmpeg -hide_banner -loglevel warning -y `
  -framerate 60 -start_number 0 -i (Join-Path $frames 'frame_%04d.png') `
  -vf $detailSelect -fps_mode vfr -frames:v 1 -update 1 $detailReview
if ($LASTEXITCODE -ne 0) { throw 'Detail review sheet build failed.' }

Copy-Item -LiteralPath (Join-Path $frames 'frame_0229.png') -Destination $poster -Force

& $ffmpeg -hide_banner -loglevel error -i $video -f null NUL
if ($LASTEXITCODE -ne 0) { throw 'Decoded MP4 verification failed.' }

$videoInfo = Get-Item -LiteralPath $video
$reviewInfo = Get-Item -LiteralPath $review
$hash = (Get-FileHash -LiteralPath $video -Algorithm SHA256).Hash
$resolvedFrames = (Resolve-Path -LiteralPath $frames).Path
$expectedFrames = [System.IO.Path]::GetFullPath((Join-Path $root '_frames'))
if ($resolvedFrames -ne $expectedFrames) { throw "Unexpected frame cleanup target: $resolvedFrames" }
Get-ChildItem -LiteralPath $resolvedFrames -Filter 'frame_*.png' -File | Remove-Item -Force
Write-Output "VIDEO=$($videoInfo.FullName)"
Write-Output "VIDEO_BYTES=$($videoInfo.Length)"
Write-Output "VIDEO_SHA256=$hash"
Write-Output "REVIEW=$($reviewInfo.FullName)"
Write-Output "DETAIL_REVIEW=$detailReview"
Write-Output 'SPEC=1280x720 / 60fps / 4.8s / H.264 + AAC'
