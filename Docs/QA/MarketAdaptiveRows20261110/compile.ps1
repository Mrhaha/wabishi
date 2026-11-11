param([switch]$IncludeValidation)
$ErrorActionPreference = 'Stop'
$marketProjectRoot = [System.IO.Path]::GetFullPath((Join-Path $PSScriptRoot '../../..'))
$marketUnityData = 'F:\unity\Unity20190433_release\Editor\Data'
$marketCompileArgs = @('/nologo', '/target:library', '/nostdlib+', '/langversion:7.3', '/define:UNITY_EDITOR;UNITY_STANDALONE_WIN', ('/out:' + (Join-Path $PSScriptRoot 'market_ui_validation.dll')))
$marketCompileArgs += Get-ChildItem -LiteralPath (Join-Path $marketUnityData 'MonoBleedingEdge/lib/mono/4.7.1-api') -Filter '*.dll' | ForEach-Object { '/reference:' + $_.FullName }
$marketCompileArgs += Get-ChildItem -LiteralPath (Join-Path $marketUnityData 'Managed/UnityEngine') -Filter '*.dll' | ForEach-Object { '/reference:' + $_.FullName }
$marketCompileArgs += Get-ChildItem -LiteralPath (Join-Path $marketProjectRoot 'Assets/Scripts') -Filter '*.cs' | ForEach-Object { $_.FullName }
if ($IncludeValidation) {
    $marketCompileArgs += '/reference:' + (Join-Path $marketUnityData 'Managed/UnityEditor.dll')
    $marketEditorModules = Join-Path $marketUnityData 'Managed/UnityEditor'
    if (Test-Path -LiteralPath $marketEditorModules) {
        $marketCompileArgs += Get-ChildItem -LiteralPath $marketEditorModules -Filter '*.dll' | ForEach-Object { '/reference:' + $_.FullName }
    }
    $marketCompileArgs += Join-Path $marketProjectRoot 'Assets/Editor/WabishMarketVariableUiCapture.cs'
}
& (Join-Path $marketUnityData 'Tools/Roslyn/csc.exe') @marketCompileArgs 2>&1 | Tee-Object -FilePath (Join-Path $PSScriptRoot 'compile.log')
exit $LASTEXITCODE
