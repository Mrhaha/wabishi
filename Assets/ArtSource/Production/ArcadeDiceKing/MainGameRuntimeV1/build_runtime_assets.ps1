param(
    [string]$ProjectRoot = (Resolve-Path (Join-Path $PSScriptRoot "../../../../..")).Path
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

Add-Type -AssemblyName System.Drawing

$sourceDir = $PSScriptRoot
$runtimeDir = Join-Path $ProjectRoot "Assets/Resources/Art/MainGame"
New-Item -ItemType Directory -Force -Path $runtimeDir | Out-Null

$cleanPlateSource = Join-Path $sourceDir "arcade_main_game_common_clean_plate_source_v1_20260715.png"
$shellAtlasSource = Join-Path $sourceDir "arcade_main_game_family_shells_chromakey_source_v1_20260715.png"
$cleanPlateTarget = Join-Path $runtimeDir "arcade_main_game_common_base.png"

function New-ArgbBitmap([int]$Width, [int]$Height) {
    return New-Object System.Drawing.Bitmap($Width, $Height, [System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
}

function Save-Png([System.Drawing.Bitmap]$Bitmap, [string]$Path) {
    $Bitmap.Save($Path, [System.Drawing.Imaging.ImageFormat]::Png)
}

function Resize-Image([string]$SourcePath, [string]$TargetPath, [int]$Width, [int]$Height) {
    $source = [System.Drawing.Image]::FromFile($SourcePath)
    try {
        $target = New-ArgbBitmap $Width $Height
        try {
            $graphics = [System.Drawing.Graphics]::FromImage($target)
            try {
                $graphics.Clear([System.Drawing.Color]::Transparent)
                $graphics.CompositingMode = [System.Drawing.Drawing2D.CompositingMode]::SourceCopy
                $graphics.CompositingQuality = [System.Drawing.Drawing2D.CompositingQuality]::HighQuality
                $graphics.InterpolationMode = [System.Drawing.Drawing2D.InterpolationMode]::HighQualityBicubic
                $graphics.PixelOffsetMode = [System.Drawing.Drawing2D.PixelOffsetMode]::HighQuality
                $graphics.SmoothingMode = [System.Drawing.Drawing2D.SmoothingMode]::HighQuality
                $graphics.DrawImage($source, (New-Object System.Drawing.Rectangle(0, 0, $Width, $Height)))
            }
            finally {
                $graphics.Dispose()
            }

            Save-Png $target $TargetPath
        }
        finally {
            $target.Dispose()
        }
    }
    finally {
        $source.Dispose()
    }
}

function Convert-ChromaPixel([System.Drawing.Color]$Color) {
    $r = [double]$Color.R
    $g = [double]$Color.G
    $b = [double]$Color.B
    $maxRb = [Math]::Max($r, $b)

    $greenDifference = $g - $maxRb
    if ($g -gt 180.0 -and $greenDifference -ge 120.0) {
        return [System.Drawing.Color]::FromArgb(0, 0, 0, 0)
    }

    if ($g -gt 100.0 -and $greenDifference -gt 45.0) {
        $alpha = 1.0 - (($greenDifference - 45.0) / 75.0)
        $alpha = [Math]::Max(0.0, [Math]::Min(1.0, $alpha))
        if ($alpha -le 0.01) {
            return [System.Drawing.Color]::FromArgb(0, 0, 0, 0)
        }

        $outR = [Math]::Max(0.0, [Math]::Min(255.0, $r / $alpha))
        $outG = [Math]::Max(0.0, [Math]::Min(255.0, ($g - ((1.0 - $alpha) * 255.0)) / $alpha))
        $outB = [Math]::Max(0.0, [Math]::Min(255.0, $b / $alpha))
        return [System.Drawing.Color]::FromArgb(
            [int][Math]::Round($alpha * 255.0),
            [int][Math]::Round($outR),
            [int][Math]::Round($outG),
            [int][Math]::Round($outB))
    }

    return [System.Drawing.Color]::FromArgb(255, $Color.R, $Color.G, $Color.B)
}

function Export-ShellSprites([string]$SourcePath, [string]$TargetDirectory) {
    $names = @("neutral", "pig", "turtle", "devil", "pirate")
    # The accepted atlas source has five cleanly separated subjects, but its generated
    # margins are not mathematically equal. These split points sit in the green gaps and
    # keep the devil and pirate silhouettes from leaking into one another.
    $splitPoints = @(0, 430, 820, 1210, 1590, 2040)
    $source = New-Object System.Drawing.Bitmap($SourcePath)
    try {
        for ($index = 0; $index -lt $names.Count; $index++) {
            $cellX = [Math]::Min($source.Width, $splitPoints[$index])
            $cellRight = [Math]::Min($source.Width, $splitPoints[$index + 1])
            $cell = New-ArgbBitmap ($cellRight - $cellX) $source.Height
            try {
                $minX = $cell.Width
                $minY = $cell.Height
                $maxX = -1
                $maxY = -1

                for ($y = 0; $y -lt $source.Height; $y++) {
                    for ($x = $cellX; $x -lt $cellRight; $x++) {
                        $converted = Convert-ChromaPixel $source.GetPixel($x, $y)
                        $localX = $x - $cellX
                        $cell.SetPixel($localX, $y, $converted)
                        if ($converted.A -gt 16) {
                            $minX = [Math]::Min($minX, $localX)
                            $minY = [Math]::Min($minY, $y)
                            $maxX = [Math]::Max($maxX, $localX)
                            $maxY = [Math]::Max($maxY, $y)
                        }
                    }
                }

                if ($maxX -lt $minX -or $maxY -lt $minY) {
                    throw "No non-keyed pixels found for family shell '$($names[$index])'."
                }

                $bounds = New-Object System.Drawing.Rectangle(
                    [Math]::Max(0, $minX - 3),
                    [Math]::Max(0, $minY - 3),
                    [Math]::Min($cell.Width - [Math]::Max(0, $minX - 3), ($maxX - $minX + 1) + 6),
                    [Math]::Min($cell.Height - [Math]::Max(0, $minY - 3), ($maxY - $minY + 1) + 6))

                $target = New-ArgbBitmap 512 512
                try {
                    $graphics = [System.Drawing.Graphics]::FromImage($target)
                    try {
                        $graphics.Clear([System.Drawing.Color]::Transparent)
                        $graphics.CompositingMode = [System.Drawing.Drawing2D.CompositingMode]::SourceCopy
                        $graphics.CompositingQuality = [System.Drawing.Drawing2D.CompositingQuality]::HighQuality
                        $graphics.InterpolationMode = [System.Drawing.Drawing2D.InterpolationMode]::HighQualityBicubic
                        $graphics.PixelOffsetMode = [System.Drawing.Drawing2D.PixelOffsetMode]::HighQuality
                        $graphics.SmoothingMode = [System.Drawing.Drawing2D.SmoothingMode]::HighQuality

                        $available = 464.0
                        $scale = [Math]::Min($available / $bounds.Width, $available / $bounds.Height)
                        $drawWidth = [int][Math]::Round($bounds.Width * $scale)
                        $drawHeight = [int][Math]::Round($bounds.Height * $scale)
                        $drawX = [int][Math]::Round((512 - $drawWidth) * 0.5)
                        $drawY = [int][Math]::Round((512 - $drawHeight) * 0.5)
                        $destination = New-Object System.Drawing.Rectangle($drawX, $drawY, $drawWidth, $drawHeight)
                        $graphics.DrawImage($cell, $destination, $bounds, [System.Drawing.GraphicsUnit]::Pixel)
                    }
                    finally {
                        $graphics.Dispose()
                    }

                    $targetPath = Join-Path $TargetDirectory ("arcade_main_game_die_shell_{0}.png" -f $names[$index])
                    Save-Png $target $targetPath
                }
                finally {
                    $target.Dispose()
                }
            }
            finally {
                $cell.Dispose()
            }
        }
    }
    finally {
        $source.Dispose()
    }
}

Resize-Image $cleanPlateSource $cleanPlateTarget 1920 1080
Export-ShellSprites $shellAtlasSource $runtimeDir

Get-ChildItem -LiteralPath $runtimeDir -Filter "arcade_main_game_*.png" |
    Sort-Object Name |
    Select-Object Name, Length
