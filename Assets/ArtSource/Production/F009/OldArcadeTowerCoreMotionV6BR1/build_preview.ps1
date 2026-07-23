param(
    [string]$Node = "C:\Users\admin\.cache\codex-runtimes\codex-primary-runtime\dependencies\node\bin\node.exe",
    [string]$NodeModules = "C:\Users\admin\.cache\codex-runtimes\codex-primary-runtime\dependencies\node\node_modules",
    [string]$Python = "C:\Users\admin\.cache\codex-runtimes\codex-primary-runtime\dependencies\python\python.exe",
    [string]$Ffmpeg = "C:\Users\admin\.codex\tools\wabish-motion-preview\node_modules\ffmpeg-static\ffmpeg.exe"
)

$ErrorActionPreference = "Stop"
$root = Split-Path -Parent $MyInvocation.MyCommand.Path
$frames = Join-Path $root "_frames"
$audio = Join-Path $root "f009_v6b_r1_old_arcade_tower_core_temp_audio_20260721.wav"
$video = Join-Path $root "f009_v6b_r1_old_arcade_tower_core_motion_preview_20260721.mp4"
$gif = Join-Path $root "f009_v6b_r1_old_arcade_tower_core_motion_preview_20260721.gif"
$webp = Join-Path $root "f009_v6b_r1_old_arcade_tower_core_motion_fullframe_60fps_20260721.webp"
$slowVideo = Join-Path $root "f009_v6b_r1_old_arcade_tower_core_motion_slow_review_60fps_20260721.mp4"
$review = Join-Path $root "f009_v6b_r1_old_arcade_tower_core_motion_review_20260721.png"
$poster = Join-Path $root "f009_v6b_r1_old_arcade_tower_core_motion_poster_20260721.png"
$fps = 60
$duration = 1.5
$frameCount = [int]($fps * $duration)

if (-not (Test-Path -LiteralPath $Node)) { throw "Node 不存在：$Node" }
if (-not (Test-Path -LiteralPath $Python)) { throw "Python 不存在：$Python" }
if (-not (Test-Path -LiteralPath $Ffmpeg)) { throw "ffmpeg 不存在：$Ffmpeg" }

New-Item -ItemType Directory -Force -Path $frames | Out-Null
$env:NODE_PATH = $NodeModules

& $Node (Join-Path $root "render_preview.js") --fps $fps
if ($LASTEXITCODE -ne 0) { throw "浏览器帧渲染失败：$LASTEXITCODE" }

& $Node (Join-Path $root "build_audio.js")
if ($LASTEXITCODE -ne 0) { throw "临时声音生成失败：$LASTEXITCODE" }

& $Ffmpeg -y -hide_banner -loglevel error `
    -framerate $fps -start_number 0 -i (Join-Path $frames "frame_%04d.png") `
    -i $audio -frames:v $frameCount -t $duration -c:v libx264 -preset slow -crf 17 -r $fps `
    -pix_fmt yuv420p -c:a aac -b:a 160k -movflags +faststart -shortest $video
if ($LASTEXITCODE -ne 0) { throw "MP4 编码失败：$LASTEXITCODE" }

& $Ffmpeg -y -hide_banner -loglevel error -i $video `
    -filter_complex "[0:v]fps=30,scale=960:540:flags=lanczos,split[a][b];[a]palettegen=max_colors=256:stats_mode=diff[p];[b][p]paletteuse=dither=bayer:bayer_scale=2:diff_mode=rectangle" `
    -loop 0 $gif
if ($LASTEXITCODE -ne 0) { throw "GIF 编码失败：$LASTEXITCODE" }

& $Python (Join-Path $root "build_fullframe_webp.py")
if ($LASTEXITCODE -ne 0) { throw "WebP 编码失败：$LASTEXITCODE" }

& $Ffmpeg -y -hide_banner -loglevel error -i $video `
    -filter_complex "[0:v]setpts=2.0*PTS,fps=60,tpad=stop_mode=clone:stop_duration=0.034[v];[0:a]atempo=0.5[a]" `
    -map "[v]" -map "[a]" -frames:v 180 -t 3.0 -r $fps -c:v libx264 -preset slow -crf 17 `
    -pix_fmt yuv420p -c:a aac -b:a 160k -movflags +faststart $slowVideo
if ($LASTEXITCODE -ne 0) { throw "慢放 MP4 编码失败：$LASTEXITCODE" }

$reviewFilter = "select='eq(n,0)+eq(n,10)+eq(n,20)+eq(n,30)+eq(n,40)+eq(n,50)+eq(n,59)+eq(n,70)+eq(n,89)',scale=426:240:flags=lanczos,tile=3x3:padding=0:margin=0:color=black"
& $Ffmpeg -y -hide_banner -loglevel error -i $video -vf $reviewFilter -vsync 0 -frames:v 1 $review
if ($LASTEXITCODE -ne 0) { throw "九帧评审图生成失败：$LASTEXITCODE" }

& $Ffmpeg -y -hide_banner -loglevel error -ss 0.96 -i $video -frames:v 1 $poster
if ($LASTEXITCODE -ne 0) { throw "海报帧生成失败：$LASTEXITCODE" }

& $Ffmpeg -v error -i $video -f null NUL
if ($LASTEXITCODE -ne 0) { throw "视频解码校验失败：$LASTEXITCODE" }

& $Ffmpeg -v error -i $slowVideo -f null NUL
if ($LASTEXITCODE -ne 0) { throw "慢放视频解码校验失败：$LASTEXITCODE" }

$videoInfo = & $Ffmpeg -hide_banner -i $video 2>&1
Write-Output ($videoInfo | Select-String -Pattern "Duration:" | Select-Object -First 1)
Write-Output ($videoInfo | Select-String -Pattern "Video:" | Select-Object -First 1)
Write-Output ($videoInfo | Select-String -Pattern "Audio:" | Select-Object -First 1)
Write-Output "DONE"
Write-Output $video
Write-Output $slowVideo
Write-Output $webp
Write-Output $gif
Write-Output $review
Write-Output $poster
