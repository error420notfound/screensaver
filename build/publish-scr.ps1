param(
    [string]$Configuration = "Release",
    [string]$Runtime = "win-x64",
    [string]$InstallRoot = "$env:LOCALAPPDATA\MujiScreenSaver\App"
)

$ErrorActionPreference = "Stop"
$repoRoot = Split-Path -Parent $PSScriptRoot
$project = Join-Path $repoRoot "src\MujiScreenSaver\MujiScreenSaver.csproj"
$publishDir = Join-Path $repoRoot "artifacts\publish\$Runtime"

dotnet publish $project -c $Configuration -r $Runtime --self-contained false -o $publishDir

New-Item -ItemType Directory -Force -Path $InstallRoot | Out-Null
Copy-Item -Path (Join-Path $publishDir "*") -Destination $InstallRoot -Recurse -Force

$exePath = Join-Path $InstallRoot "MujiScreenSaver.exe"
$scrPath = Join-Path $InstallRoot "MujiScreenSaver.scr"
Copy-Item -Path $exePath -Destination $scrPath -Force

Write-Host "Published screensaver to $scrPath"
return $scrPath
