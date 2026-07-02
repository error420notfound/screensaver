param(
    [string]$Configuration = "Release",
    [string]$Runtime = "win-x64"
)

$ErrorActionPreference = "Stop"
$scrPath = & (Join-Path $PSScriptRoot "publish-scr.ps1") -Configuration $Configuration -Runtime $Runtime

Set-ItemProperty -Path "HKCU:\Control Panel\Desktop" -Name "SCRNSAVE.EXE" -Value $scrPath
Set-ItemProperty -Path "HKCU:\Control Panel\Desktop" -Name "ScreenSaveActive" -Value "1"

Write-Host "Registered current-user screensaver:"
Write-Host $scrPath
