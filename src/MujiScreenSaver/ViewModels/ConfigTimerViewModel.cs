using MujiScreenSaver.Models;
using MujiScreenSaver.Services;

namespace MujiScreenSaver.ViewModels;

public sealed class ConfigTimerViewModel : ViewModelBase
{
    private readonly Func<DateTimeOffset> _now;
    private readonly TimerStateCalculator _calculator;
    private readonly Action _save;
    private TimerDefinition _definition;
    private string _statusText = string.Empty;
    private double _progress;

    public ConfigTimerViewModel(TimerDefinition definition, Func<DateTimeOffset> now, TimerStateCalculator calculator, Action save)
    {
        _definition = definition.Clone();
        _now = now;
        _calculator = calculator;
        _save = save;
        StartCommand = new RelayCommand(Start);
        PauseCommand = new RelayCommand(Pause);
        ResetCommand = new RelayCommand(Reset);
        RefreshStatus();
    }

    public RelayCommand StartCommand { get; }

    public RelayCommand PauseCommand { get; }

    public RelayCommand ResetCommand { get; }

    public string Id => _definition.Id;

    public string Label
    {
        get => _definition.Label;
        set
        {
            if (_definition.Label == value)
            {
                return;
            }

            _definition.Label = value;
            OnPropertyChanged();
            _save();
        }
    }

    public int DurationMinutes
    {
        get => Math.Max(1, (int)Math.Ceiling(_definition.DurationSeconds / 60.0));
        set
        {
            var durationSeconds = Math.Clamp(value, 1, 24 * 60) * 60;
            if (_definition.DurationSeconds == durationSeconds)
            {
                return;
            }

            _definition.DurationSeconds = durationSeconds;
            var state = _calculator.Calculate(_definition, _now());
            if (state.ElapsedSeconds >= durationSeconds)
            {
                _definition.ElapsedSeconds = durationSeconds;
                _definition.StartedAt = null;
                _definition.IsRunning = false;
            }

            OnPropertyChanged();
            RefreshStatus();
            _save();
        }
    }

    public bool IsVisible
    {
        get => _definition.IsVisible;
        set
        {
            if (_definition.IsVisible == value)
            {
                return;
            }

            _definition.IsVisible = value;
            OnPropertyChanged();
            _save();
        }
    }

    public string StatusText
    {
        get => _statusText;
        private set => SetProperty(ref _statusText, value);
    }

    public double Progress
    {
        get => _progress;
        private set => SetProperty(ref _progress, value);
    }

    public TimerDefinition ToDefinition() => _definition.Clone();

    public void RefreshStatus()
    {
        var state = _calculator.Calculate(_definition, _now());
        StatusText = state.IsComplete
            ? "Complete"
            : $"{TimerViewModel.FormatDuration(state.RemainingSeconds)} remaining";
        Progress = state.Progress;
    }

    private void Start()
    {
        _definition = _calculator.Start(_definition, _now());
        RefreshStatus();
        OnPropertyChanged(nameof(DurationMinutes));
        _save();
    }

    private void Pause()
    {
        _definition = _calculator.Pause(_definition, _now());
        RefreshStatus();
        _save();
    }

    private void Reset()
    {
        _definition = _calculator.Reset(_definition);
        RefreshStatus();
        _save();
    }
}
