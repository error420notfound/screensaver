# Muji Screen Saver

A minimal Windows-only screensaver built with C#/.NET 8 and WPF.

## Modes

```powershell
.\MujiScreenSaver.scr /s
.\MujiScreenSaver.scr /p <HWND>
.\MujiScreenSaver.scr /c
```

No arguments open the configuration window.

## Build

Requires the .NET 8 Desktop Runtime/SDK on Windows.

```powershell
dotnet restore
dotnet build -c Release
dotnet test
.\build\publish-scr.ps1
```

`publish-scr.ps1` uses `dotnet` from `PATH` when available, then falls back to the standard `Program Files\dotnet\dotnet.exe` install locations.

The publish script copies the framework-dependent app to:

```text
%LOCALAPPDATA%\MujiScreenSaver\App
```

and creates `MujiScreenSaver.scr` beside the published executable and runtime files.

## Install For Current User

```powershell
.\build\install-current-user.ps1
```

This sets:

```text
HKCU\Control Panel\Desktop\SCRNSAVE.EXE
HKCU\Control Panel\Desktop\ScreenSaveActive
```

## Timer Data

Timer settings are stored locally at:

```text
%APPDATA%\MujiScreenSaver\timers.json
```

The file is written atomically through a temporary file and normalized on load. Corrupt JSON is preserved as `timers.corrupt.<timestamp>.json` and safe defaults are loaded.

## Notes

This project targets `net8.0-windows` with WPF and must be built and validated on Windows. Preview mode depends on the native HWND supplied by Windows Screen Saver Settings.

Validated on Windows with `dotnet restore`, `dotnet build -c Release`, `dotnet test`, publish/install/uninstall scripts, and short smoke launches for `/c`, `/s`, and `/p <HWND>`.
