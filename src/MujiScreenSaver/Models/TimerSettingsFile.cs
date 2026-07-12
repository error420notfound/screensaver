namespace MujiScreenSaver.Models;

public sealed class TimerSettingsFile
{
    public int Version { get; set; } = 2;

    public bool Use12HourTime { get; set; } = true;

    public ThemeMode ThemeMode { get; set; } = ThemeMode.System;

    public ColorPalette ColorPalette { get; set; } = ColorPalette.Orange;

    public List<TimerDefinition> Timers { get; set; } = [];
}
