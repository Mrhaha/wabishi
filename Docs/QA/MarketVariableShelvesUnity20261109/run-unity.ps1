param([ValidateSet('Validate','Capture')][string]$Mode = 'Validate')
$ErrorActionPreference = 'Stop'
$marketWorkerRoot = Join-Path ([System.IO.Path]::GetTempPath()) 'wabish-market-ui-20261109'
$marketUnityExecutable = 'F:\unity\Unity20190433_release\Editor\Unity.exe'
$marketLogPath = Join-Path $PSScriptRoot ($Mode.ToLowerInvariant() + '-unity.log')
$marketArguments = @('-batchmode', '-projectPath', ('"' + $marketWorkerRoot + '"'), '-executeMethod', ('WabishMarketVariableUiCapture.' + $Mode), '-outputDir', ('"' + $PSScriptRoot + '"'), '-logFile', ('"' + $marketLogPath + '"'))
if ($Mode -eq 'Validate') { $marketArguments += '-nographics' }
else { $marketArguments += @('-force-d3d11', '-isolatedMarketUiValidation', 'true') }
$marketValidationProcess = Start-Process -FilePath $marketUnityExecutable -ArgumentList $marketArguments -WindowStyle Hidden -PassThru
$marketValidationProcess.Id | Set-Content -LiteralPath (Join-Path $PSScriptRoot ($Mode.ToLowerInvariant() + '-pid.txt'))
$marketValidationProcess.WaitForExit()
$marketExitCode = $marketValidationProcess.ExitCode
[PSCustomObject]@{ Mode=$Mode; ExitCode=$marketExitCode; Worker=$marketWorkerRoot; Log=$marketLogPath } | ConvertTo-Json -Compress | Set-Content -LiteralPath (Join-Path $PSScriptRoot ($Mode.ToLowerInvariant() + '-process.json'))
Get-Content -LiteralPath (Join-Path $PSScriptRoot ($Mode.ToLowerInvariant() + '-process.json'))
exit $marketExitCode
