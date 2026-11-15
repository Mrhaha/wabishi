param([string]$WorkerRoot = (Join-Path ([System.IO.Path]::GetTempPath()) 'wabish-market-ui-20261109'))
$ErrorActionPreference = 'Stop'
$marketProjectRoot = [System.IO.Path]::GetFullPath((Join-Path $PSScriptRoot '../../..'))
$marketWorkerRoot = [System.IO.Path]::GetFullPath($WorkerRoot)
$marketTempRoot = [System.IO.Path]::GetFullPath([System.IO.Path]::GetTempPath()).TrimEnd('\') + '\'
if (-not $marketWorkerRoot.StartsWith($marketTempRoot, [System.StringComparison]::OrdinalIgnoreCase) -or -not ([System.IO.Path]::GetFileName($marketWorkerRoot)).StartsWith('wabish-market-ui-')) {
    throw ('拒绝写入预期临时范围外的验证工程：' + $marketWorkerRoot)
}
New-Item -ItemType Directory -Path $marketWorkerRoot -Force | Out-Null
foreach ($marketFolder in @('Assets/Scripts', 'Assets/Resources', 'Assets/Scenes', 'Packages', 'ProjectSettings')) {
    $marketSourceFolder = Join-Path $marketProjectRoot $marketFolder
    $marketTargetFolder = Join-Path $marketWorkerRoot $marketFolder
    New-Item -ItemType Directory -Path $marketTargetFolder -Force | Out-Null
    & robocopy $marketSourceFolder $marketTargetFolder /E /COPY:DAT /R:1 /W:1 /NFL /NDL /NJH /NJS /NP | Out-Null
    if ($LASTEXITCODE -ge 8) { throw ('复制验证目录失败：' + $marketFolder) }
}
$marketPackageCache = Join-Path $marketProjectRoot 'Library/PackageCache'
if (Test-Path -LiteralPath $marketPackageCache) {
    & robocopy $marketPackageCache (Join-Path $marketWorkerRoot 'Library/PackageCache') /E /COPY:DAT /R:1 /W:1 /NFL /NDL /NJH /NJS /NP | Out-Null
    if ($LASTEXITCODE -ge 8) { throw '复制现有包缓存失败。' }
}
$marketEditorFolder = Join-Path $marketWorkerRoot 'Assets/Editor'
New-Item -ItemType Directory -Path $marketEditorFolder -Force | Out-Null
foreach ($marketRunnerFile in @('WabishMarketVariableUiCapture.cs', 'WabishMarketVariableUiCapture.cs.meta')) {
    $marketRunnerSource = Join-Path $marketProjectRoot ('Assets/Editor/' + $marketRunnerFile)
    if (Test-Path -LiteralPath $marketRunnerSource) { Copy-Item -LiteralPath $marketRunnerSource -Destination (Join-Path $marketEditorFolder $marketRunnerFile) -Force }
}
$marketSettingsPath = Join-Path $marketWorkerRoot 'ProjectSettings/ProjectSettings.asset'
$marketSettingsText = [System.IO.File]::ReadAllText($marketSettingsPath)
$marketSettingsText = [System.Text.RegularExpressions.Regex]::Replace($marketSettingsText, '(?m)^  companyName:.*$', '  companyName: CodexWabishMarketUiQA')
$marketSettingsText = [System.Text.RegularExpressions.Regex]::Replace($marketSettingsText, '(?m)^  productName:.*$', '  productName: WabishMarketUi20261109')
[System.IO.File]::WriteAllText($marketSettingsPath, $marketSettingsText, [System.Text.UTF8Encoding]::new($false))
[System.IO.File]::WriteAllText((Join-Path $marketWorkerRoot 'ProjectSettings/ProjectVersion.txt'), "m_EditorVersion: 2019.4.33f1`n", [System.Text.UTF8Encoding]::new($false))
[PSCustomObject]@{ WorkerRoot = $marketWorkerRoot; Company = 'CodexWabishMarketUiQA'; Product = 'WabishMarketUi20261109'; RuntimeResourcesCopied = $true } | ConvertTo-Json -Compress
exit 0
