$ErrorActionPreference = "Stop"
$desktopKey = "HKCU:\Control Panel\Desktop"
$current = (Get-ItemProperty -Path $desktopKey -Name "SCRNSAVE.EXE" -ErrorAction SilentlyContinue)."SCRNSAVE.EXE"

if ($current -and (Split-Path $current -Leaf) -ieq "MujiScreenSaver.scr") {
    Remove-ItemProperty -Path $desktopKey -Name "SCRNSAVE.EXE" -ErrorAction SilentlyContinue
    Write-Host "Removed MujiScreenSaver screensaver registration."
} else {
    Write-Host "MujiScreenSaver is not the registered screensaver."
}
