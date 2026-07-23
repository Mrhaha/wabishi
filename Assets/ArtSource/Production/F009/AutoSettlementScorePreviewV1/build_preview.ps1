param(
    [string]$Node = "C:\Users\admin\.cache\codex-runtimes\codex-primary-runtime\dependencies\node\bin\node.exe",
    [string]$NodeModules = "C:\Users\admin\.cache\codex-runtimes\codex-primary-runtime\dependencies\node\node_modules",
    [string]$Ffmpeg = "C:\Users\admin\.codex\tools\wabish-motion-preview\node_modules\ffmpeg-static\ffmpeg.exe"
)

$ErrorActionPreference = "Stop"
$root = Split-Path -Parent $MyInvocation.MyCommand.Path
$frames = Join-Path $root "_frames"
$tierFrames = Join-Path $root "_tier_frames"
$video = Join-Path $root "f009_auto_settlement_score_tiers_preview_v1_20260717.mp4"
$gif = Join-Path $root "f009_auto_settlement_score_tiers_preview_v1_20260717.gif"
$review = Join-Path $root "f009_auto_settlement_score_tiers_review_v1_20260717.png"
$tierReview = Join-Path $root "f009_score_tier_states_review_v1_20260717.png"

if (-not (Test-Path -LiteralPath $Node)) { throw "Node 不存在：$Node" }
if (-not (Test-Path -LiteralPath $Ffmpeg)) { throw "ffmpeg 不存在：$Ffmpeg" }

New-Item -ItemType Directory -Force -Path $frames | Out-Null
$env:NODE_PATH = $NodeModules
& $Node (Join-Path $root "render_preview.js")
if ($LASTEXITCODE -ne 0) { throw "浏览器帧渲染失败：$LASTEXITCODE" }

& $Ffmpeg -y -hide_banner -loglevel error `
    -framerate 30 -start_number 0 -i (Join-Path $frames "frame_%04d.jpg") `
    -frames:v 186 -c:v libx264 -preset slow -crf 17 -pix_fmt yuv420p -movflags +faststart -an $video
if ($LASTEXITCODE -ne 0) { throw "MP4 编码失败：$LASTEXITCODE" }

& $Ffmpeg -y -hide_banner -loglevel error -i $video `
    -filter_complex "[0:v]fps=15,scale=960:540:flags=lanczos,split[a][b];[a]palettegen=max_colors=160:stats_mode=diff[p];[b][p]paletteuse=dither=bayer:bayer_scale=3:diff_mode=rectangle" `
    -loop 0 $gif
if ($LASTEXITCODE -ne 0) { throw "GIF 编码失败：$LASTEXITCODE" }

$reviewFilter = "select='eq(n,3)+eq(n,15)+eq(n,31)+eq(n,44)+eq(n,70)+eq(n,88)+eq(n,106)+eq(n,126)+eq(n,165)',scale=640:360:flags=lanczos,tile=3x3"
& $Ffmpeg -y -hide_banner -loglevel error -i $video -vf $reviewFilter -vsync 0 -frames:v 1 $review
if ($LASTEXITCODE -ne 0) { throw "评审总览图生成失败：$LASTEXITCODE" }

& $Ffmpeg -y -hide_banner -loglevel error -framerate 1 -start_number 0 -i (Join-Path $tierFrames "tier_%02d.png") `
    -vf "scale=640:360:flags=lanczos,tile=3x2:padding=0:margin=0:color=black" -frames:v 1 $tierReview
if ($LASTEXITCODE -ne 0) { throw "五级终局总览图生成失败：$LASTEXITCODE" }

& $Ffmpeg -v error -i $video -f null NUL
if ($LASTEXITCODE -ne 0) { throw "视频解码校验失败：$LASTEXITCODE" }

Write-Output "DONE"
Write-Output $video
Write-Output $gif
Write-Output $review
Write-Output $tierReview
