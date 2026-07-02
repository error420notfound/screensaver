namespace MujiScreenSaver.Models;

public sealed class TimerDefinition
{
    public string Id { get; set; } = string.Empty;

    public string Label { get; set; } = string.Empty;

    public int DurationSeconds { get; set; } = 300;

    public string? StartedAt { get; set; }

    public int ElapsedSeconds { get; set; }

    public bool IsRunning { get; set; }

    public bool IsVisible { get; set; }

    public TimerDefinition Clone()
    {
        return new TimerDefinition
        {
            Id = Id,
            Label = Label,
            DurationSeconds = DurationSeconds,
            StartedAt = StartedAt,
            ElapsedSeconds = ElapsedSeconds,
            IsRunning = IsRunning,
            IsVisible = IsVisible
        };
    }
}
