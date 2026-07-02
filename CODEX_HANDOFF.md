# Codex Handoff: Muji Screen Saver

## Current State

This repo now contains a Windows-only C#/.NET 8 WPF screensaver implementation scaffold and source code.

The original workspace had only an empty `index.html`; the implemented project now includes:

- `MujiScreenSaver.sln`
- WPF app under `src/MujiScreenSaver`
- Unit tests under `tests/MujiScreenSaver.Tests`
- Windows publish/install scripts under `build`
- README with build/install notes

The app is intended to build into a framework-dependent Windows `.scr` screensaver.

## Product Goal

Create a minimal, Muji-inspired Windows screensaver that shows:

- Current date, e.g. `THURSDAY, 02 JULY 2026`
- Monday-first week strip with dates
- Current local time
- Up to three timers
- Quiet horizontal timer progress bars

The visual character should be calm, sparse, legible from a distance, and restrained. It should avoid cards, shadows, gradients, bright colors, icons, sounds, and flashing states.

## Architecture

Use pure WPF, not WebView2 or Electron.

Core modes:

- `/s`: full-screen screensaver mode
- `/p <HWND>` or `/p:<HWND>`: Windows Screen Saver Settings preview host mode
- `/c`: small timer configuration/control window
- no args: configuration window

Important files:

- `src/MujiScreenSaver/Program.cs`: entry point and mode dispatch
- `src/MujiScreenSaver/Modes/CommandLineOptions.cs`: screensaver argument parsing
- `src/MujiScreenSaver/Modes/ScreenSaverMode.cs`: full-screen multi-monitor launch
- `src/MujiScreenSaver/Modes/PreviewMode.cs`: preview mode
- `src/MujiScreenSaver/Interop/PreviewHost.cs`: `HwndSource` child host for `/p`
- `src/MujiScreenSaver/Interop/NativeMethods.cs`: Win32 interop
- `src/MujiScreenSaver/Views/ScreenSaverView.xaml`: shared display surface
- `src/MujiScreenSaver/Views/ConfigWindow.xaml`: configuration window
- `src/MujiScreenSaver/Services/TimerStore.cs`: JSON storage
- `src/MujiScreenSaver/Services/TimerStateCalculator.cs`: timer math
- `src/MujiScreenSaver/Services/WeekCalendarService.cs`: date/week formatting
- `build/publish-scr.ps1`: publish and create `.scr`
- `build/install-current-user.ps1`: register current-user screensaver

## Data And Persistence

Timer data is stored at:

```text
%APPDATA%\MujiScreenSaver\timers.json
```

Model shape:

```json
{
  "version": 1,
  "timers": [
    {
      "id": "focus",
      "label": "Focus",
      "durationSeconds": 3600,
      "startedAt": "2026-07-02T10:00:00+05:30",
      "elapsedSeconds": 0,
      "isRunning": true,
      "isVisible": true
    }
  ]
}
```

Persistence behavior:

- Maximum three timers
- Missing timers are filled with hidden defaults
- Corrupt JSON is renamed to `timers.corrupt.<timestamp>.json`
- Atomic writes use `timers.json.tmp`, `File.Replace`, and `timers.json.bak` where possible
- Screensaver display reloads timer state every 3 seconds

Timer behavior:

- Running elapsed = stored `elapsedSeconds` plus `DateTimeOffset.Now - startedAt`
- Paused timers use stored elapsed only
- Sleep/wake naturally counts elapsed wall-clock time
- Time zone changes are handled through `DateTimeOffset`
- Backward clock shifts clamp negative deltas to zero
- Complete timers display `Complete`, full bar, no sound or flashing

## Visual Direction

Implemented in `ScreenSaverView.xaml`.

Design tokens:

- Background: `#F3F0E8`
- Primary text: `#24231F`
- Muted secondary: `#747067`
- Rules/tracks: `#CFC9BD`
- Font: `Segoe UI Variable Display/Text`, fallback `Segoe UI`

Keep the design sparse. Do not add cards, shadows, glass, gradients, icons, dense panels, or decorative animation.

## Known Validation Gap

The previous implementation environment was macOS and did not have `dotnet`, `msbuild`, or `pwsh` available. Static checks were done, but the project has not yet been compiled or run on Windows in this chat.

Completed checks:

- `git diff --check`
- XML/XAML well-formedness via `xmllint`
- Source scans for unsupported or unwanted technology

Required next validation on Windows:

```powershell
dotnet restore
dotnet build -c Release
dotnet test
.\build\publish-scr.ps1
.\build\install-current-user.ps1
```

## Suggested Next Work

1. Build on Windows and fix compile errors.
2. Run unit tests and fix failing tests.
3. Launch config mode:

   ```powershell
   .\MujiScreenSaver.scr /c
   ```

4. Launch full-screen mode:

   ```powershell
   .\MujiScreenSaver.scr /s
   ```

5. Register and test Windows Screen Saver Settings preview mode.
6. Validate multi-monitor behavior on real or virtual multi-display Windows setup.
7. Validate mixed-DPI behavior.
8. Tune the WPF visual layout after seeing it on 1080p and 4K displays.

## Technical Risks To Check First

- WPF custom entry point may need generated `App.g.cs` entry-point suppression/coordination.
- `/p` preview mode is the highest-risk area because it depends on native HWND child hosting.
- Full-screen focus-loss exit must not self-trigger across multiple screensaver windows.
- DPI conversion in monitor bounds may need adjustment after real Windows testing.
- Framework-dependent `.scr` packaging must keep `.dll`, `.deps.json`, and `.runtimeconfig.json` next to the renamed `.scr`.

## Original Implementation Plan

Phases:

1. Static visual prototype
2. Live date/time/week
3. Timers and JSON persistence
4. Configuration window
5. `.scr` packaging, preview mode, and multi-monitor validation

The current repo has implementation coverage for all five phases, but still needs Windows build/runtime validation and any fixes revealed by that environment.
