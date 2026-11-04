param([string]$Scratch = 'D:\AI_work_space\tmp\agent-scratch\20261104-194014-maa-tutorial-dev', [switch]$ForceRestore)
$ErrorActionPreference = 'Stop'
$projectRoot = Split-Path $PSScriptRoot -Parent
$sdk = Join-Path $projectRoot 'dependencies\dotnet-sdk-8.0.427\dotnet.exe'
if (-not (Test-Path -LiteralPath $sdk)) { & (Join-Path $PSScriptRoot 'Bootstrap.ps1') -Scratch $Scratch }
$env:DOTNET_CLI_HOME = Join-Path $Scratch 'dotnet-home'
$env:NUGET_PACKAGES = Join-Path $Scratch 'nuget-packages'
$env:NUGET_HTTP_CACHE_PATH = Join-Path $Scratch 'nuget-http-cache'
$env:NUGET_PLUGINS_CACHE_PATH = Join-Path $Scratch 'nuget-plugin-cache'
$env:DOTNET_CLI_TELEMETRY_OPTOUT = '1'
$env:DOTNET_SKIP_FIRST_TIME_EXPERIENCE = '1'
$env:DOTNET_GENERATE_ASPNET_CERTIFICATE = 'false'
$env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH = 'false'
$env:TEMP = Join-Path $Scratch 'runtime-temp'
$env:TMP = $env:TEMP
New-Item -ItemType Directory -Path $env:TEMP -Force | Out-Null
$output = Join-Path $projectRoot 'releases\current'
if ($ForceRestore) {
    & $sdk restore (Join-Path $projectRoot 'src\MaaTutorialAssistant.csproj') --force --locked-mode
    if ($LASTEXITCODE -ne 0) { throw 'Dependency restore failed' }
}
& $sdk publish (Join-Path $projectRoot 'src\MaaTutorialAssistant.csproj') -c Release --self-contained false -o $output "-p:MaaScratch=$Scratch"
if ($LASTEXITCODE -ne 0) { throw 'Build failed' }
$models = Join-Path $projectRoot 'dependencies\ocr'
& (Join-Path $PSScriptRoot 'Prepare-Models.ps1') -Scratch $Scratch
if (Test-Path -LiteralPath $models) {
    $modelOutput = Join-Path $output 'resources\model'
    New-Item -ItemType Directory -Path $modelOutput -Force | Out-Null
    Copy-Item -LiteralPath $models -Destination $modelOutput -Recurse -Force
}
Copy-Item -LiteralPath (Join-Path $projectRoot 'licenses') -Destination $output -Recurse -Force
foreach ($notice in @('LICENSE','THIRD_PARTY_NOTICES.md','README.md')) {
    Copy-Item -LiteralPath (Join-Path $projectRoot $notice) -Destination $output -Force
}
Write-Output $output
