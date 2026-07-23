param(
    [string]$ProjectRoot = (Resolve-Path (Join-Path $PSScriptRoot "../../../../..")).Path
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

Add-Type -AssemblyName System.Drawing

$sourcePath = Join-Path $PSScriptRoot "arcade_market_common_clean_plate_source_v1_20260717.png"
$runtimeDirectory = Join-Path $ProjectRoot "Assets/Resources/Art/Market"
$targetPath = Join-Path $runtimeDirectory "arcade_market_common_base.png"

New-Item -ItemType Directory -Force -Path $runtimeDirectory | Out-Null

$source = [System.Drawing.Image]::FromFile($sourcePath)
try {
    $target = New-Object System.Drawing.Bitmap(1920, 1080, [System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
    try {
        $graphics = [System.Drawing.Graphics]::FromImage($target)
        try {
            $graphics.Clear([System.Drawing.Color]::Black)
            $graphics.CompositingMode = [System.Drawing.Drawing2D.CompositingMode]::SourceCopy
            $graphics.CompositingQuality = [System.Drawing.Drawing2D.CompositingQuality]::HighQuality
            $graphics.InterpolationMode = [System.Drawing.Drawing2D.InterpolationMode]::HighQualityBicubic
            $graphics.PixelOffsetMode = [System.Drawing.Drawing2D.PixelOffsetMode]::HighQuality
            $graphics.SmoothingMode = [System.Drawing.Drawing2D.SmoothingMode]::HighQuality
            $graphics.DrawImage($source, (New-Object System.Drawing.Rectangle(0, 0, 1920, 1080)))
        }
        finally {
            $graphics.Dispose()
        }

        $target.Save($targetPath, [System.Drawing.Imaging.ImageFormat]::Png)
    }
    finally {
        $target.Dispose()
    }
}
finally {
    $source.Dispose()
}

$image = [System.Drawing.Image]::FromFile($targetPath)
try {
    if ($image.Width -ne 1920 -or $image.Height -ne 1080) {
        throw "Unexpected market runtime texture size: $($image.Width)x$($image.Height)."
    }
}
finally {
    $image.Dispose()
}

Get-Item -LiteralPath $targetPath | Select-Object Name, Length
Get-FileHash -LiteralPath $targetPath -Algorithm SHA256 | Select-Object Algorithm, Hash
