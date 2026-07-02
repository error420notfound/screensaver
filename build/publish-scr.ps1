param(
    [string]$Configuration = "Release",
    [string]$Runtime = "win-x64",
    [string]$InstallRoot = "$env:LOCALAPPDATA\MujiScreenSaver\App"
)

$ErrorActionPreference = "Stop"
$repoRoot = Split-Path -Parent $PSScriptRoot
$project = Join-Path $repoRoot "src\MujiScreenSaver\MujiScreenSaver.csproj"
$publishDir = Join-Path $repoRoot "artifacts\publish\$Runtime"

function Resolve-DotNet {
    $command = Get-Command dotnet -ErrorAction SilentlyContinue
    if ($command) {
        return $command.Source
    }

    $programFilesPath = Join-Path $env:ProgramFiles "dotnet\dotnet.exe"
    if (Test-Path $programFilesPath) {
        return $programFilesPath
    }

    $programFilesX86 = [Environment]::GetFolderPath("ProgramFilesX86")
    if ($programFilesX86) {
        $programFilesX86Path = Join-Path $programFilesX86 "dotnet\dotnet.exe"
        if (Test-Path $programFilesX86Path) {
            return $programFilesX86Path
        }
    }

    throw "dotnet was not found on PATH or in the standard Program Files install locations."
}

$dotnet = Resolve-DotNet
$publish = Start-Process `
    -FilePath $dotnet `
    -ArgumentList @("publish", "`"$project`"", "-c", $Configuration, "-r", $Runtime, "--self-contained", "false", "-o", "`"$publishDir`"") `
    -NoNewWindow `
    -Wait `
    -PassThru
if ($publish.ExitCode -ne 0) {
    throw "dotnet publish failed with exit code $($publish.ExitCode)."
}

if (-not (Test-Path $publishDir)) {
    throw "dotnet publish did not create the expected output directory: $publishDir"
}

New-Item -ItemType Directory -Force -Path $InstallRoot | Out-Null
Copy-Item -Path (Join-Path $publishDir "*") -Destination $InstallRoot -Recurse -Force

$exePath = Join-Path $InstallRoot "MujiScreenSaver.exe"
$scrPath = Join-Path $InstallRoot "MujiScreenSaver.scr"
Copy-Item -Path $exePath -Destination $scrPath -Force

Write-Host "Published screensaver to $scrPath"
return $scrPath
