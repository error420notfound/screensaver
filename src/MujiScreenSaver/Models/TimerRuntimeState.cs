namespace MujiScreenSaver.Models;

public sealed record TimerRuntimeState(
    string Id,
    string Label,
    int DurationSeconds,
    int ElapsedSeconds,
    int RemainingSeconds,
    bool IsRunning,
    bool IsVisible,
    bool IsComplete,
    double Progress);
