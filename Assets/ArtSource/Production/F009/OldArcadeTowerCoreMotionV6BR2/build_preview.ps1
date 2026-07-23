$ErrorActionPreference = 'Stop'

$root = Split-Path -Parent $MyInvocation.MyCommand.Path
$node = 'C:\Users\admin\.cache\codex-runtimes\codex-primary-runtime\dependencies\node\bin\node.exe'
$nodeModules = 'C:\Users\admin\.cache\codex-runtimes\codex-primary-runtime\dependencies\node\node_modules'
$ffmpeg = 'C:\Users\admin\.codex\tools\wabish-motion-preview\node_modules\ffmpeg-static\ffmpeg.exe'
$frames = Join-Path $root '_frames'
$audio = Join-Path $root 'f009_v6b_r2_full_chain_critical_temp_audio_20260721.wav'
$video = Join-Path $root 'f009_v6b_r2_full_chain_critical_preview_20260721.mp4'
$review = Join-Path $root 'f009_v6b_r2_full_chain_critical_review_20260721.png'
$poster = Join-Path $root 'f009_v6b_r2_full_chain_critical_poster_20260721.png'

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

$select = "select='eq(n,0)+eq(n,18)+eq(n,51)+eq(n,72)+eq(n,93)+eq(n,114)+eq(n,126)+eq(n,149)+eq(n,167)+eq(n,206)+eq(n,227)+eq(n,270)',scale=320:180,tile=4x3:padding=4:margin=4:color=0x080705"
& $ffmpeg -hide_banner -loglevel warning -y `
  -framerate 60 -start_number 0 -i (Join-Path $frames 'frame_%04d.png') `
  -vf $select -fps_mode vfr -frames:v 1 -update 1 $review
if ($LASTEXITCODE -ne 0) { throw 'Review sheet build failed.' }

Copy-Item -LiteralPath (Join-Path $frames 'frame_0227.png') -Destination $poster -Force

& $ffmpeg -hide_banner -loglevel error -i $video -f null NUL
if ($LASTEXITCODE -ne 0) { throw 'Decoded MP4 verification failed.' }

$videoInfo = Get-Item -LiteralPath $video
$reviewInfo = Get-Item -LiteralPath $review
$hash = (Get-FileHash -LiteralPath $video -Algorithm SHA256).Hash
Write-Output "VIDEO=$($videoInfo.FullName)"
Write-Output "VIDEO_BYTES=$($videoInfo.Length)"
Write-Output "VIDEO_SHA256=$hash"
Write-Output "REVIEW=$($reviewInfo.FullName)"
Write-Output 'SPEC=1280x720 / 60fps / 4.8s / H.264 + AAC'
