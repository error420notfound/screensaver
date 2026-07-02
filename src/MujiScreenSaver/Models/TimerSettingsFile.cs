namespace MujiScreenSaver.Models;

public sealed class TimerSettingsFile
{
    public int Version { get; set; } = 1;

    public List<TimerDefinition> Timers { get; set; } = [];
}
