param(
    [Parameter(Mandatory)][string]$Source,
    [string]$Scratch = (Join-Path ([IO.Path]::GetTempPath()) 'maa-tutorial-assistant-build')
)
$ErrorActionPreference = 'Stop'
$projectRoot = Split-Path $PSScriptRoot -Parent
$fixture = Join-Path $projectRoot 'test-environments\maa-v6.21.3'
if (Test-Path -LiteralPath $fixture) { throw 'Fixture already exists. Use Reset-TestFixture.ps1 for this project fixture.' }
$sourceRoot = (Resolve-Path -LiteralPath $Source).Path.TrimEnd('\')
if (-not (Test-Path -LiteralPath (Join-Path $sourceRoot 'MAA.exe') -PathType Leaf)) { throw 'Source is not an MAA installation.' }
New-Item -ItemType Directory -Path $fixture, $Scratch -Force | Out-Null
$manifest = @(foreach ($file in Get-ChildItem -LiteralPath $sourceRoot -Recurse -File -Force) {
    $stream = [IO.File]::OpenRead($file.FullName)
    try { $hash = [Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($stream)) } finally { $stream.Dispose() }
    [pscustomobject]@{ Path = $file.FullName.Substring($sourceRoot.Length + 1); Bytes = $file.Length; SHA256 = $hash }
})
$manifest | Export-Csv -LiteralPath (Join-Path $projectRoot 'test-environments\original-files.csv') -NoTypeInformation -Encoding utf8
& robocopy $sourceRoot $fixture /E /COPY:DAT /DCOPY:DAT /XJ /R:1 /W:1 /NFL /NDL /NP /NJH /NJS ('/LOG:' + (Join-Path $Scratch 'fixture-copy.log')) | Out-Null
if ($LASTEXITCODE -ge 8) { throw 'Fixture copy failed' }
foreach ($entry in $manifest) {
    $path = Join-Path $fixture $entry.Path
    $stream = [IO.File]::OpenRead($path)
    try { $hash = [Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($stream)) } finally { $stream.Dispose() }
    if ($hash -ne $entry.SHA256) { throw ('Fixture mismatch: ' + $entry.Path) }
}
Copy-Item -LiteralPath (Join-Path $fixture 'config\gui.new.json') -Destination (Join-Path $projectRoot 'test-environments\baseline-config.json')
[pscustomobject]@{ Source = $sourceRoot; Fixture = $fixture; Created = (Get-Date -Format o); Files = $manifest.Count; Bytes = ($manifest | Measure-Object Bytes -Sum).Sum } |
    ConvertTo-Json | Set-Content -LiteralPath (Join-Path $fixture '.maa-tutorial-fixture.json') -Encoding utf8NoBOM
& (Join-Path $PSScriptRoot 'Reset-TestFixture.ps1') -Theme Dark
