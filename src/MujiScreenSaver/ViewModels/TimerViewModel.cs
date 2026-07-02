using MujiScreenSaver.Models;

namespace MujiScreenSaver.ViewModels;

public sealed class TimerViewModel : ViewModelBase
{
    private string _label = string.Empty;
    private string _remainingText = string.Empty;
    private string _elapsedTotalText = string.Empty;
    private double _progress;
    private bool _isVisible;

    public string Label
    {
        get => _label;
        set => SetProperty(ref _label, value);
    }

    public string RemainingText
    {
        get => _remainingText;
        set => SetProperty(ref _remainingText, value);
    }

    public string ElapsedTotalText
    {
        get => _elapsedTotalText;
        set => SetProperty(ref _elapsedTotalText, value);
    }

    public double Progress
    {
        get => _progress;
        set => SetProperty(ref _progress, value);
    }

    public bool IsVisible
    {
        get => _isVisible;
        set => SetProperty(ref _isVisible, value);
    }

    public void Update(TimerRuntimeState state)
    {
        Label = state.Label;
        RemainingText = state.IsComplete ? "Complete" : FormatDuration(state.RemainingSeconds);
        ElapsedTotalText = $"{FormatDuration(state.ElapsedSeconds)} / {FormatDuration(state.DurationSeconds)}";
        Progress = state.Progress;
        IsVisible = state.IsVisible;
    }

    public static string FormatDuration(int totalSeconds)
    {
        totalSeconds = Math.Max(0, totalSeconds);
        var time = TimeSpan.FromSeconds(totalSeconds);

        if (time.TotalHours >= 1)
        {
            return $"{(int)time.TotalHours:0}:{time.Minutes:00}:{time.Seconds:00}";
        }

        return $"{time.Minutes:00}:{time.Seconds:00}";
    }
}
