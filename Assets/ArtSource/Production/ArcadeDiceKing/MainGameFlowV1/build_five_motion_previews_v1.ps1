param(
    [string]$Ffmpeg = "C:\Users\admin\.codex\tools\wabish-motion-preview\node_modules\ffmpeg-static\ffmpeg.exe"
)

$ErrorActionPreference = "Stop"

$root = Split-Path -Parent $MyInvocation.MyCommand.Path
$contact = Join-Path $root "arcade_dice_king_main_game_nine_state_contact_v2_led_fit_20260716.png"
$outputDir = Join-Path $root "MotionPreviewsV1"
$sourceDir = Join-Path $outputDir "_sources"
$posterDir = Join-Path $outputDir "_posters"

New-Item -ItemType Directory -Force -Path $outputDir, $sourceDir, $posterDir | Out-Null

if (-not (Test-Path -LiteralPath $Ffmpeg)) {
    throw "ffmpeg 不存在：$Ffmpeg"
}
if (-not (Test-Path -LiteralPath $contact)) {
    throw "九状态接触图不存在：$contact"
}

function Invoke-Ffmpeg {
    param([string[]]$Arguments)
    & $Ffmpeg @Arguments
    if ($LASTEXITCODE -ne 0) {
        throw "ffmpeg 执行失败，退出码：$LASTEXITCODE"
    }
}

function Render-Frame {
    param(
        [string]$InputPath,
        [string]$Filter,
        [string]$OutputPath
    )
    Invoke-Ffmpeg @(
        "-y",
        "-hide_banner",
        "-loglevel", "error",
        "-i", $InputPath,
        "-vf", $Filter,
        "-frames:v", "1",
        $OutputPath
    )
}

function Render-Color-Frame {
    param(
        [string]$Filter,
        [string]$OutputPath
    )
    Invoke-Ffmpeg @(
        "-y",
        "-hide_banner",
        "-loglevel", "error",
        "-f", "lavfi",
        "-i", "color=c=black:s=1920x1080:d=0.1",
        "-vf", $Filter,
        "-frames:v", "1",
        $OutputPath
    )
}

function Build-Xfade-Video {
    param(
        [string[]]$Inputs,
        [double[]]$Durations,
        [string[]]$Transitions,
        [double[]]$TransitionDurations,
        [string]$OutputPath
    )

    if ($Inputs.Count -ne $Durations.Count) {
        throw "输入帧与持续时间数量不一致"
    }
    if ($Transitions.Count -ne ($Inputs.Count - 1)) {
        throw "转场数量不正确"
    }
    if ($TransitionDurations.Count -ne ($Inputs.Count - 1)) {
        throw "转场时间数量不正确"
    }

    $arguments = @("-y", "-hide_banner", "-loglevel", "error")
    for ($i = 0; $i -lt $Inputs.Count; $i++) {
        $arguments += @(
            "-loop", "1",
            "-framerate", "30",
            "-t", $Durations[$i].ToString("0.###", [Globalization.CultureInfo]::InvariantCulture),
            "-i", $Inputs[$i]
        )
    }

    $parts = [System.Collections.Generic.List[string]]::new()
    for ($i = 0; $i -lt $Inputs.Count; $i++) {
        $parts.Add("[$($i):v]fps=30,scale=1920:1080:flags=lanczos,setsar=1,settb=AVTB,format=yuv420p,setpts=PTS-STARTPTS[v$i]")
    }

    $timeline = $Durations[0]
    $current = "v0"
    for ($i = 1; $i -lt $Inputs.Count; $i++) {
        $fade = $TransitionDurations[$i - 1]
        $offset = $timeline - $fade
        $offsetText = $offset.ToString("0.###", [Globalization.CultureInfo]::InvariantCulture)
        $fadeText = $fade.ToString("0.###", [Globalization.CultureInfo]::InvariantCulture)
        $next = "x$i"
        $parts.Add("[$current][v$i]xfade=transition=$($Transitions[$i - 1]):duration=$($fadeText):offset=$($offsetText)[$next]")
        $current = $next
        $timeline = $timeline + $Durations[$i] - $fade
    }
    $parts.Add("[$current]format=yuv420p[vout]")

    $filter = [string]::Join(";", $parts)
    $totalText = $timeline.ToString("0.###", [Globalization.CultureInfo]::InvariantCulture)
    $arguments += @(
        "-filter_complex", $filter,
        "-map", "[vout]",
        "-t", $totalText,
        "-r", "30",
        "-c:v", "libx264",
        "-preset", "slow",
        "-crf", "18",
        "-pix_fmt", "yuv420p",
        "-movflags", "+faststart",
        "-an",
        $OutputPath
    )
    Invoke-Ffmpeg $arguments
}

$tileSpecs = @(
    @{ Name = "01_ready"; X = 0; Y = 0 },
    @{ Name = "02_shaking"; X = 560; Y = 0 },
    @{ Name = "03_stopping_slot3"; X = 1120; Y = 0 },
    @{ Name = "04_result_locked"; X = 0; Y = 315 },
    @{ Name = "05_scoring_slot2"; X = 560; Y = 315 },
    @{ Name = "06_target_crossed"; X = 1120; Y = 315 },
    @{ Name = "07_stage_clear"; X = 0; Y = 630 },
    @{ Name = "08_stage_failed"; X = 560; Y = 630 },
    @{ Name = "09_run_over"; X = 1120; Y = 630 }
)

$frames = @{}
foreach ($spec in $tileSpecs) {
    $output = Join-Path $sourceDir "$($spec.Name)_1920x1080.png"
    $filter = "crop=560:315:$($spec.X):$($spec.Y),scale=1920:1080:flags=lanczos,unsharp=5:5:0.45:3:3:0.15"
    Render-Frame -InputPath $contact -Filter $filter -OutputPath $output
    $frames[$spec.Name] = $output
}

$clearBase = Join-Path $sourceDir "07_stage_clear_heading_only.png"
$clearFixed = Join-Path $sourceDir "07_stage_clear_fixed.png"
$clearInterest = Join-Path $sourceDir "07_stage_clear_interest.png"
Render-Frame -InputPath $frames["07_stage_clear"] -Filter "drawbox=x=1575:y=350:w=310:h=260:color=0x050606@0.99:t=fill" -OutputPath $clearBase
Render-Frame -InputPath $frames["07_stage_clear"] -Filter "drawbox=x=1575:y=425:w=310:h=185:color=0x050606@0.99:t=fill" -OutputPath $clearFixed
Render-Frame -InputPath $frames["07_stage_clear"] -Filter "drawbox=x=1575:y=500:w=310:h=110:color=0x050606@0.99:t=fill" -OutputPath $clearInterest

$runStage1 = Join-Path $sourceDir "09_run_over_power_stage1.png"
$runStage2 = Join-Path $sourceDir "09_run_over_power_stage2.png"
$runStage3 = Join-Path $sourceDir "09_run_over_power_stage3.png"
Render-Frame -InputPath $frames["09_run_over"] -Filter "drawbox=x=1570:y=180:w=320:h=720:color=black@0.78:t=fill,drawbox=x=620:y=915:w=700:h=145:color=black@0.82:t=fill" -OutputPath $runStage1
Render-Frame -InputPath $runStage1 -Filter "drawbox=x=340:y=325:w=1130:h=340:color=black@0.88:t=fill" -OutputPath $runStage2
Render-Frame -InputPath $runStage2 -Filter "drawbox=x=0:y=0:w=1920:h=1080:color=black@0.56:t=fill,eq=saturation=0.30:brightness=-0.18" -OutputPath $runStage3

$lineWide = Join-Path $sourceDir "09_signal_line_wide.png"
$lineMid = Join-Path $sourceDir "09_signal_line_mid.png"
$lineNarrow = Join-Path $sourceDir "09_signal_line_narrow.png"
$black = Join-Path $sourceDir "09_black.png"
Render-Color-Frame -Filter "drawbox=x=150:y=535:w=1620:h=10:color=0x69d6d7@0.18:t=fill,drawbox=x=160:y=538:w=1600:h=4:color=0xe2b252@0.92:t=fill" -OutputPath $lineWide
Render-Color-Frame -Filter "drawbox=x=595:y=536:w=730:h=8:color=0x69d6d7@0.16:t=fill,drawbox=x=610:y=539:w=700:h=3:color=0xe2b252@0.88:t=fill" -OutputPath $lineMid
Render-Color-Frame -Filter "drawbox=x=860:y=537:w=200:h=6:color=0x69d6d7@0.15:t=fill,drawbox=x=870:y=539:w=180:h=2:color=0xe2b252@0.82:t=fill" -OutputPath $lineNarrow
Render-Color-Frame -Filter "null" -OutputPath $black

$scanline = Join-Path $sourceDir "08_retry_scanline.png"
Invoke-Ffmpeg @(
    "-y", "-hide_banner", "-loglevel", "error",
    "-f", "lavfi",
    "-i", "color=c=0x73d8d6@0.72:s=1920x8:d=0.1,format=rgba",
    "-frames:v", "1",
    $scanline
)

$rollStop = Join-Path $outputDir "arcade_dice_king_roll_to_stop_preview_v1_20260716.mp4"
Build-Xfade-Video -Inputs @($frames["01_ready"], $frames["02_shaking"], $frames["03_stopping_slot3"], $frames["04_result_locked"]) -Durations @(1.0, 2.0, 1.2, 2.2) -Transitions @("fade", "wipeleft", "wipeleft") -TransitionDurations @(0.20, 0.65, 0.55) -OutputPath $rollStop

$scoringCross = Join-Path $outputDir "arcade_dice_king_scoring_target_cross_preview_v1_20260716.mp4"
Build-Xfade-Video -Inputs @($frames["04_result_locked"], $frames["05_scoring_slot2"], $frames["06_target_crossed"]) -Durations @(1.0, 2.4, 3.0) -Transitions @("fade", "wipeleft") -TransitionDurations @(0.25, 0.55) -OutputPath $scoringCross

$clearIncome = Join-Path $outputDir "arcade_dice_king_stage_clear_income_preview_v1_20260716.mp4"
Build-Xfade-Video -Inputs @($frames["06_target_crossed"], $clearBase, $clearFixed, $clearInterest, $frames["07_stage_clear"]) -Durations @(1.0, 0.9, 0.8, 0.8, 2.4) -Transitions @("fade", "fade", "fade", "fade") -TransitionDurations @(0.25, 0.08, 0.08, 0.08) -OutputPath $clearIncome

$retryBase = Join-Path $sourceDir "arcade_dice_king_stage_failed_retry_base_v1_20260716.mp4"
Build-Xfade-Video -Inputs @($frames["08_stage_failed"], $frames["01_ready"]) -Durations @(3.0, 2.5) -Transitions @("wipedown") -TransitionDurations @(0.75) -OutputPath $retryBase

$retry = Join-Path $outputDir "arcade_dice_king_stage_failed_retry_preview_v1_20260716.mp4"
Invoke-Ffmpeg @(
    "-y", "-hide_banner", "-loglevel", "error",
    "-i", $retryBase,
    "-loop", "1", "-framerate", "30", "-t", "4.75", "-i", $scanline,
    "-filter_complex", "[1:v]format=rgba,settb=AVTB,setpts=PTS+2.25/TB[line];[0:v][line]overlay=x=0:y='(t-2.25)/0.75*(H-h)':enable='between(t,2.25,3.0)':eval=frame:shortest=1,format=yuv420p[vout]",
    "-map", "[vout]",
    "-t", "4.75",
    "-r", "30",
    "-c:v", "libx264",
    "-preset", "slow",
    "-crf", "18",
    "-pix_fmt", "yuv420p",
    "-movflags", "+faststart",
    "-an",
    $retry
)

$runOver = Join-Path $outputDir "arcade_dice_king_run_over_powerdown_preview_v1_20260716.mp4"
Build-Xfade-Video -Inputs @($frames["09_run_over"], $runStage1, $runStage2, $runStage3, $lineWide, $lineMid, $lineNarrow, $black) -Durations @(1.8, 0.8, 0.8, 0.8, 0.45, 0.35, 0.35, 1.2) -Transitions @("fade", "fade", "fade", "fade", "fade", "fade", "fade") -TransitionDurations @(0.20, 0.20, 0.20, 0.15, 0.12, 0.12, 0.12) -OutputPath $runOver

$videos = @(
    @{ Path = $rollStop; Time = "2.5" },
    @{ Path = $scoringCross; Time = "3.2" },
    @{ Path = $clearIncome; Time = "3.7" },
    @{ Path = $retry; Time = "2.7" },
    @{ Path = $runOver; Time = "3.7" }
)

$posterPaths = @()
for ($i = 0; $i -lt $videos.Count; $i++) {
    $poster = Join-Path $posterDir ("{0:D2}_poster.png" -f ($i + 1))
    Invoke-Ffmpeg @(
        "-y", "-hide_banner", "-loglevel", "error",
        "-ss", $videos[$i].Time,
        "-i", $videos[$i].Path,
        "-vf", "scale=384:216:flags=lanczos",
        "-frames:v", "1",
        $poster
    )
    $posterPaths += $poster
}

$reviewSheet = Join-Path $outputDir "arcade_dice_king_five_motion_previews_review_sheet_v1_20260716.png"
$reviewArgs = @("-y", "-hide_banner", "-loglevel", "error")
foreach ($poster in $posterPaths) {
    $reviewArgs += @("-i", $poster)
}
$reviewArgs += @(
    "-filter_complex", "[0:v][1:v][2:v][3:v][4:v]hstack=inputs=5[vout]",
    "-map", "[vout]",
    "-frames:v", "1",
    $reviewSheet
)
Invoke-Ffmpeg $reviewArgs

foreach ($video in $videos) {
    & $Ffmpeg -v error -i $video.Path -f null NUL
    if ($LASTEXITCODE -ne 0) {
        throw "视频解码校验失败：$($video.Path)"
    }
    $info = & $Ffmpeg -hide_banner -i $video.Path 2>&1
    Write-Output ($info | Select-String -Pattern "Duration:" | Select-Object -First 1)
    Write-Output ($info | Select-String -Pattern "Video:" | Select-Object -First 1)
}

Write-Output "DONE"
Write-Output $rollStop
Write-Output $scoringCross
Write-Output $clearIncome
Write-Output $retry
Write-Output $runOver
Write-Output $reviewSheet
