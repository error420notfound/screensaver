using System.Globalization;
using MujiScreenSaver.Models;

namespace MujiScreenSaver.Services;

public sealed class TimerStateCalculator
{
    public const int MaximumTimers = 3;
    public const int DefaultDurationSeconds = 300;

    public TimerSettingsFile Normalize(TimerSettingsFile? file)
    {
        var normalized = new TimerSettingsFile
        {
            Version = 2,
            Use12HourTime = file?.Use12HourTime ?? true,
            ThemeMode = NormalizeThemeMode(file?.ThemeMode),
            ColorPalette = NormalizeColorPalette(file?.ColorPalette)
        };
        var incoming = file?.Timers ?? [];

        for (var index = 0; index < MaximumTimers; index++)
        {
            var source = index < incoming.Count ? incoming[index] : null;
            normalized.Timers.Add(NormalizeTimer(source, index));
        }

        return normalized;
    }

    private static ThemeMode NormalizeThemeMode(ThemeMode? value)
    {
        var candidate = value ?? ThemeMode.System;
        return Enum.IsDefined(candidate) ? candidate : ThemeMode.System;
    }

    private static ColorPalette NormalizeColorPalette(ColorPalette? value)
    {
        var candidate = value ?? ColorPalette.Orange;
        return Enum.IsDefined(candidate) ? candidate : ColorPalette.Orange;
    }

    public TimerRuntimeState Calculate(TimerDefinition timer, DateTimeOffset now)
    {
        var duration = Math.Max(1, timer.DurationSeconds);
        var elapsed = Math.Clamp(timer.ElapsedSeconds, 0, duration);
        var isRunning = timer.IsRunning;

        if (isRunning)
        {
            if (TryParseStartedAt(timer.StartedAt, out var startedAt))
            {
                var runningSeconds = (int)Math.Floor((now - startedAt).TotalSeconds);
                elapsed = Math.Clamp(elapsed + Math.Max(0, runningSeconds), 0, duration);
            }
            else
            {
                isRunning = false;
            }
        }

        var isComplete = elapsed >= duration;
        if (isComplete)
        {
            elapsed = duration;
            isRunning = false;
        }

        var remaining = Math.Max(0, duration - elapsed);
        var progress = duration == 0 ? 1 : Math.Clamp((double)elapsed / duration, 0, 1);

        return new TimerRuntimeState(
            timer.Id,
            timer.Label,
            duration,
            elapsed,
            remaining,
            isRunning,
            timer.IsVisible,
            isComplete,
            progress);
    }

    public TimerDefinition Start(TimerDefinition timer, DateTimeOffset now)
    {
        var state = Calculate(timer, now);
        return new TimerDefinition
        {
            Id = timer.Id,
            Label = timer.Label,
            DurationSeconds = Math.Max(1, timer.DurationSeconds),
            ElapsedSeconds = state.IsComplete ? 0 : state.ElapsedSeconds,
            StartedAt = now.ToString("O", CultureInfo.InvariantCulture),
            IsRunning = true,
            IsVisible = timer.IsVisible
        };
    }

    public TimerDefinition Pause(TimerDefinition timer, DateTimeOffset now)
    {
        var state = Calculate(timer, now);
        return new TimerDefinition
        {
            Id = timer.Id,
            Label = timer.Label,
            DurationSeconds = Math.Max(1, timer.DurationSeconds),
            ElapsedSeconds = state.ElapsedSeconds,
            StartedAt = null,
            IsRunning = false,
            IsVisible = timer.IsVisible
        };
    }

    public TimerDefinition Reset(TimerDefinition timer)
    {
        return new TimerDefinition
        {
            Id = timer.Id,
            Label = timer.Label,
            DurationSeconds = Math.Max(1, timer.DurationSeconds),
            ElapsedSeconds = 0,
            StartedAt = null,
            IsRunning = false,
            IsVisible = timer.IsVisible
        };
    }

    private static TimerDefinition NormalizeTimer(TimerDefinition? source, int index)
    {
        var id = string.IsNullOrWhiteSpace(source?.Id) ? $"timer-{index + 1}" : source.Id.Trim();
        var label = string.IsNullOrWhiteSpace(source?.Label) ? $"Timer {index + 1}" : source.Label.Trim();
        var duration = source?.DurationSeconds is > 0 ? source.DurationSeconds : DefaultDurationSeconds;
        var elapsed = Math.Clamp(source?.ElapsedSeconds ?? 0, 0, duration);
        var startedAt = source?.StartedAt;
        var isRunning = source?.IsRunning == true;

        if (isRunning && !TryParseStartedAt(startedAt, out _))
        {
            isRunning = false;
            startedAt = null;
        }

        if (elapsed >= duration)
        {
            elapsed = duration;
            isRunning = false;
            startedAt = null;
        }

        return new TimerDefinition
        {
            Id = id,
            Label = label,
            DurationSeconds = duration,
            StartedAt = startedAt,
            ElapsedSeconds = elapsed,
            IsRunning = isRunning,
            IsVisible = source?.IsVisible ?? false
        };
    }

    private static bool TryParseStartedAt(string? value, out DateTimeOffset startedAt)
    {
        return DateTimeOffset.TryParse(
            value,
            CultureInfo.InvariantCulture,
            DateTimeStyles.RoundtripKind,
            out startedAt);
    }
}
