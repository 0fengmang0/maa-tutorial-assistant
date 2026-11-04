param([ValidateSet('Dark', 'Light', 'SyncWithOs')][string]$Theme = 'Dark', [switch]$Launch,
    [string]$Scratch = 'D:\AI_work_space\tmp\agent-scratch\20261104-194014-maa-tutorial-dev')
$ErrorActionPreference = 'Stop'
$projectRoot = Split-Path $PSScriptRoot -Parent
$fixture = [IO.Path]::GetFullPath((Join-Path $projectRoot 'test-environments\maa-v6.21.3'))
$markerPath = Join-Path $fixture '.maa-tutorial-fixture.json'
if (-not (Test-Path -LiteralPath $markerPath)) { throw 'Only the marked project fixture can be reset.' }
$marker = Get-Content -LiteralPath $markerPath -Raw | ConvertFrom-Json
if ($marker.Fixture -ne $fixture) { throw 'Fixture identity mismatch' }
if (Get-Process -Name MAA -ErrorAction SilentlyContinue | Where-Object { $_.Path -eq (Join-Path $fixture 'MAA.exe') }) {
    throw 'Close the project fixture before resetting its test configuration.'
}
$config = Get-Content -LiteralPath (Join-Path $projectRoot 'test-environments\baseline-config.json') -Raw | ConvertFrom-Json
$config.Gui.GuideStep = 0
$config.Gui.GuideTutorialVersion = 2
$config.Gui.DarkMode = $Theme
$config.Gui.MinimizeOnStartup = $false
$config.Update.CheckOnStartup = $false
$config.Update.CheckOnSchedule = $false
$config.Update.AutoDownloadUpdatePackage = $false
$config.Update.AutoInstallUpdatePackage = $false
$config.Update.UpdatePackage = ''
$config.Timers.ForceScheduledStart = $false
$config.Timers.ScheduledWakeUp = $false
foreach ($timer in $config.Timers.List) { $timer.IsEnabled = $false }
$config | ConvertTo-Json -Depth 100 | Set-Content -LiteralPath (Join-Path $fixture 'config\gui.new.json') -Encoding utf8NoBOM
if ($Launch) {
    $env:TEMP = Join-Path $Scratch 'fixture-runtime-temp'
    $env:TMP = $env:TEMP
    New-Item -ItemType Directory -Path $env:TEMP -Force | Out-Null
    $launchId = Get-Date -Format 'yyyyMMdd-HHmmss'
    Start-Process -FilePath (Join-Path $fixture 'MAA.exe') -WorkingDirectory $fixture -ArgumentList '--skip-core-init', '--skip-startup-auto-run' -WindowStyle Normal -RedirectStandardOutput (Join-Path $Scratch "fixture-$launchId.stdout.log") -RedirectStandardError (Join-Path $Scratch "fixture-$launchId.stderr.log") | Out-Null
}
Write-Output ('Fixture ready: ' + $fixture + ' / ' + $Theme)
