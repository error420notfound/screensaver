using MujiScreenSaver.Models;
using MujiScreenSaver.Services;
using Xunit;

namespace MujiScreenSaver.Tests;

public sealed class TimerStateCalculatorTests
{
    private readonly TimerStateCalculator _calculator = new();

    [Fact]
    public void RunningTimerUsesStartedAtAndStoredElapsed()
    {
        var started = new DateTimeOffset(2026, 7, 2, 10, 0, 0, TimeSpan.FromHours(5.5));
        var now = started.AddMinutes(10);
        var timer = new TimerDefinition
        {
            Id = "focus",
            Label = "Focus",
            DurationSeconds = 3600,
            ElapsedSeconds = 120,
            StartedAt = started.ToString("O"),
            IsRunning = true,
            IsVisible = true
        };

        var state = _calculator.Calculate(timer, now);

        Assert.Equal(720, state.ElapsedSeconds);
        Assert.Equal(2880, state.RemainingSeconds);
        Assert.True(state.IsRunning);
    }

    [Fact]
    public void PausedTimerUsesStoredElapsedOnly()
    {
        var timer = new TimerDefinition
        {
            Id = "tea",
            Label = "Tea",
            DurationSeconds = 300,
            ElapsedSeconds = 90,
            IsRunning = false
        };

        var state = _calculator.Calculate(timer, DateTimeOffset.Now.AddHours(3));

        Assert.Equal(90, state.ElapsedSeconds);
        Assert.Equal(210, state.RemainingSeconds);
    }

    [Fact]
    public void CompletedTimerIsClampedAndQuiet()
    {
        var started = DateTimeOffset.Parse("2026-07-02T10:00:00+05:30");
        var timer = new TimerDefinition
        {
            Id = "done",
            Label = "Done",
            DurationSeconds = 60,
            StartedAt = started.ToString("O"),
            IsRunning = true
        };

        var state = _calculator.Calculate(timer, started.AddMinutes(2));

        Assert.True(state.IsComplete);
        Assert.False(state.IsRunning);
        Assert.Equal(60, state.ElapsedSeconds);
        Assert.Equal(1, state.Progress);
    }

    [Fact]
    public void BackwardClockShiftDoesNotSubtractElapsed()
    {
        var started = DateTimeOffset.Parse("2026-07-02T10:00:00+05:30");
        var timer = new TimerDefinition
        {
            Id = "clock",
            Label = "Clock",
            DurationSeconds = 600,
            ElapsedSeconds = 60,
            StartedAt = started.ToString("O"),
            IsRunning = true
        };

        var state = _calculator.Calculate(timer, started.AddMinutes(-5));

        Assert.Equal(60, state.ElapsedSeconds);
        Assert.Equal(540, state.RemainingSeconds);
    }

    [Fact]
    public void InvalidStartedAtPausesTimerDuringNormalization()
    {
        var file = new TimerSettingsFile
        {
            Timers =
            [
                new TimerDefinition
                {
                    Id = "bad",
                    Label = "Bad",
                    DurationSeconds = 300,
                    StartedAt = "not-a-date",
                    IsRunning = true
                }
            ]
        };

        var normalized = _calculator.Normalize(file);

        Assert.False(normalized.Timers[0].IsRunning);
        Assert.Null(normalized.Timers[0].StartedAt);
    }
}
