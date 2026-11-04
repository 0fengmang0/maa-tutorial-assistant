param([string]$Scratch = (Join-Path ([IO.Path]::GetTempPath()) 'maa-tutorial-assistant-build'))
$ErrorActionPreference = 'Stop'
$projectRoot = Split-Path $PSScriptRoot -Parent
$manifest = Get-Content -LiteralPath (Join-Path $projectRoot 'docs\dependency-models.json') -Raw | ConvertFrom-Json
$models = Join-Path $projectRoot 'dependencies\ocr'
New-Item -ItemType Directory -Path $Scratch,$models -Force | Out-Null
foreach ($entry in $manifest.Files) {
    $destination = Join-Path $models $entry.File
    if (-not (Test-Path -LiteralPath $destination)) {
        $download = Join-Path $Scratch ($entry.File + '.download')
        $url = 'https://raw.githubusercontent.com/MaaXYZ/MaaCommonAssets/' + $manifest.Commit + '/' + $manifest.Folder + '/' + $entry.File
        Invoke-WebRequest -Uri $url -OutFile $download
        if ((Get-FileHash -LiteralPath $download -Algorithm SHA256).Hash -ne $entry.Hash) { throw ('Model hash mismatch: ' + $entry.File) }
        Copy-Item -LiteralPath $download -Destination $destination
    }
    if ((Get-FileHash -LiteralPath $destination -Algorithm SHA256).Hash -ne $entry.Hash) { throw ('Installed model hash mismatch: ' + $entry.File) }
}
Copy-Item -LiteralPath (Join-Path $projectRoot 'licenses\MaaCommonAssets-MIT.txt') -Destination (Join-Path $models 'LICENSE') -Force
Write-Output 'Pinned Chinese OCR models verified.'
