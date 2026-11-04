param([string]$OutputDirectory)
$ErrorActionPreference = 'Stop'
$projectRoot = Split-Path $PSScriptRoot -Parent
if (-not $OutputDirectory) { $OutputDirectory = Join-Path $projectRoot 'releases\github' }
$pending = @(& git -C $projectRoot status --porcelain --untracked-files=all)
if ($LASTEXITCODE -ne 0) { throw 'Git status failed.' }
if ($pending.Count -gt 0) { throw 'Commit formal source and documentation before packaging.' }
$revision = & git -C $projectRoot rev-parse HEAD
if ($LASTEXITCODE -ne 0) { throw 'Cannot resolve source revision.' }
New-Item -ItemType Directory -Path $OutputDirectory -Force | Out-Null
$archivePath = Join-Path $OutputDirectory 'maa-tutorial-assistant-source.zip'
& git -C $projectRoot archive --format=zip --prefix=maa-tutorial-assistant/ "--output=$archivePath" HEAD
if ($LASTEXITCODE -ne 0) { throw 'Source archive failed.' }
$hash = (Get-FileHash -LiteralPath $archivePath -Algorithm SHA256).Hash.ToLowerInvariant()
"$hash  maa-tutorial-assistant-source.zip" | Set-Content -LiteralPath (Join-Path $OutputDirectory 'SHA256SUMS.txt') -Encoding utf8NoBOM
@(
    "Project: MAA Tutorial Assistant"
    "Source commit: $revision"
    "Packaged UTC: $([DateTimeOffset]::UtcNow.ToString('O'))"
    "Content: committed source and documentation only; no Git database or local runtime/user data."
    "License: MIT for original assistant source; third-party terms in THIRD_PARTY_NOTICES.md."
    "Instructions: docs/GitHub上传步骤.md"
) | Set-Content -LiteralPath (Join-Path $OutputDirectory 'SOURCE_INFO.txt') -Encoding utf8NoBOM
Write-Output $archivePath
Write-Output "SHA256: $hash"
