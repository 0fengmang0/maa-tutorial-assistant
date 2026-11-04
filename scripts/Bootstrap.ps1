param([string]$Scratch = (Join-Path ([IO.Path]::GetTempPath()) 'maa-tutorial-assistant-build'))
$ErrorActionPreference = 'Stop'
$projectRoot = Split-Path $PSScriptRoot -Parent
$sdkRoot = Join-Path $projectRoot 'dependencies\dotnet-sdk-8.0.427'
$archive = Join-Path $Scratch 'dotnet-sdk-8.0.427-win-x64.zip'
$sdkUrl = 'https://builds.dotnet.microsoft.com/dotnet/Sdk/8.0.427/dotnet-sdk-8.0.427-win-x64.zip'
$expectedHash = 'A4DD6EB2BF2A2F6FB10997D3E044A229B23AD96A5C86D20494687E34EE73B537D7BD14488CE14CDED6917F6778DA4D66360E2734020B7EF487D0F29AB144489D'
New-Item -ItemType Directory -Path $Scratch, $sdkRoot -Force | Out-Null
if (-not (Test-Path -LiteralPath (Join-Path $sdkRoot 'dotnet.exe'))) {
    if (-not (Test-Path -LiteralPath $archive)) {
        Invoke-WebRequest -Uri $sdkUrl -OutFile $archive
    }
    if ((Get-FileHash -LiteralPath $archive -Algorithm SHA512).Hash -ne $expectedHash) {
        throw 'SDK SHA512 verification failed. Retain the archive for inspection.'
    }
    Expand-Archive -LiteralPath $archive -DestinationPath $sdkRoot -Force
}
$env:DOTNET_CLI_HOME = Join-Path $Scratch 'dotnet-home'
$env:DOTNET_SKIP_FIRST_TIME_EXPERIENCE = '1'
$env:DOTNET_CLI_TELEMETRY_OPTOUT = '1'
$env:DOTNET_GENERATE_ASPNET_CERTIFICATE = 'false'
$env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH = 'false'
& (Join-Path $sdkRoot 'dotnet.exe') --version
if ($LASTEXITCODE -ne 0) { throw 'SDK startup failed' }
