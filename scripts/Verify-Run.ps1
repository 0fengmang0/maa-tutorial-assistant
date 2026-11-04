param(
    [Parameter(Mandatory)][string]$ResultDirectory,
    [Parameter(Mandatory)][ValidateSet('Dark','Light','SyncWithOs')][string]$OriginalTheme,
    [switch]$Restore,
    [switch]$CheckOriginal
)
$ErrorActionPreference = 'Stop'
$projectRoot = Split-Path $PSScriptRoot -Parent
$fixture = Join-Path $projectRoot 'test-environments\maa-v6.21.3'
$result = Get-Content -LiteralPath (Join-Path $ResultDirectory 'result.json') -Raw | ConvertFrom-Json
$receipt = @(Get-ChildItem -LiteralPath (Join-Path $ResultDirectory 'state') -Filter 'theme-*.json')
if ($receipt.Count -ne 1) { throw 'Expected one theme receipt.' }
$state = Get-Content -LiteralPath $receipt[0].FullName -Raw | ConvertFrom-Json
$config = Get-Content -LiteralPath (Join-Path $fixture 'config\gui.new.json') -Raw | ConvertFrom-Json
$expectedTheme = if ($Restore) { $OriginalTheme } else { 'Dark' }
if (-not $result.TutorialCompleted -or $config.Gui.GuideStep -ne 7) { throw 'The actual MAA tutorial is incomplete.' }
if ($state.OriginalTheme -ne $OriginalTheme -or $config.Gui.DarkMode -ne $expectedTheme) { throw 'Original/final theme mismatch.' }
if ([bool]$result.ThemeRestored -ne $Restore.IsPresent -or [bool]$state.Restored -ne $Restore.IsPresent) { throw 'Restore task result mismatch.' }
if ($CheckOriginal) {
    $marker = Get-Content -LiteralPath (Join-Path $fixture '.maa-tutorial-fixture.json') -Raw | ConvertFrom-Json
    $baseline = @(Import-Csv -LiteralPath (Join-Path $projectRoot 'test-environments\original-files.csv'))
    $current = @(Get-ChildItem -LiteralPath $marker.Source -File -Recurse -Force)
    if ($current.Count -ne $baseline.Count) { throw 'Original file count changed.' }
    foreach ($entry in $baseline) {
        $file = Get-Item -LiteralPath (Join-Path $marker.Source $entry.Path)
        $stream = [IO.File]::OpenRead($file.FullName)
        try { $hash = [Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($stream)) } finally { $stream.Dispose() }
        if ($file.Length -ne [long]$entry.Bytes -or $hash -ne $entry.SHA256) { throw ('Original installation changed: ' + $entry.Path) }
    }
    Write-Output ('Original unchanged: ' + $baseline.Count + ' files')
}
Write-Output ('PASS: actual GuideStep=7; original=' + $OriginalTheme + '; final=' + $expectedTheme + '; restore=' + $Restore.IsPresent)
