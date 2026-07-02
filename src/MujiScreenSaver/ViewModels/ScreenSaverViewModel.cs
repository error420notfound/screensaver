using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows.Threading;
using MujiScreenSaver.Models;
using MujiScreenSaver.Services;

namespace MujiScreenSaver.ViewModels;

public sealed class ScreenSaverViewModel : ViewModelBase, IDisposable
{
    private readonly ClockService _clock;
    private readonly WeekCalendarService _weekCalendar;
    private readonly TimerStore _timerStore;
    private readonly TimerStateCalculator _timerCalculator;
    private readonly DispatcherTimer _clockTimer;
    private readonly DispatcherTimer _reloadTimer;
    private TimerSettingsFile _settings;
    private string _dateText = string.Empty;
    private string _timeText = string.Empty;
    private bool _disposed;

    private ScreenSaverViewModel(
        bool isPreview,
        ClockService clock,
        WeekCalendarService weekCalendar,
        TimerStore timerStore,
        TimerStateCalculator timerCalculator)
    {
        IsPreview = isPreview;
        _clock = clock;
        _weekCalendar = weekCalendar;
        _timerStore = timerStore;
        _timerCalculator = timerCalculator;
        _settings = _timerStore.Load();

        for (var index = 0; index < 7; index++)
        {
            WeekDays.Add(new WeekDayViewModel());
        }

        for (var index = 0; index < TimerStateCalculator.MaximumTimers; index++)
        {
            Timers.Add(new TimerViewModel());
        }

        _clockTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
        _clockTimer.Tick += (_, _) => RefreshFromClock();

        _reloadTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(3) };
        _reloadTimer.Tick += (_, _) => ReloadSettings();
    }

    public bool IsPreview { get; }

    public string DateText
    {
        get => _dateText;
        private set => SetProperty(ref _dateText, value);
    }

    public string TimeText
    {
        get => _timeText;
        private set => SetProperty(ref _timeText, value);
    }

    public ObservableCollection<WeekDayViewModel> WeekDays { get; } = [];

    public ObservableCollection<TimerViewModel> Timers { get; } = [];

    public static ScreenSaverViewModel Create(bool isPreview)
    {
        var calculator = new TimerStateCalculator();
        return new ScreenSaverViewModel(
            isPreview,
            new ClockService(),
            new WeekCalendarService(),
            new TimerStore(calculator),
            calculator);
    }

    public void Start()
    {
        ReloadSettings();
        RefreshFromClock();
        _clockTimer.Start();
        _reloadTimer.Start();
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        _clockTimer.Stop();
        _reloadTimer.Stop();
    }

    private void ReloadSettings()
    {
        _settings = _timerStore.Load();
        RefreshFromClock();
    }

    private void RefreshFromClock()
    {
        var now = _clock.Now();
        DateText = _weekCalendar.FormatDate(now);
        TimeText = now.LocalDateTime.ToString(IsPreview ? "HH:mm" : "HH:mm:ss", CultureInfo.InvariantCulture);

        var week = _weekCalendar.GetMondayFirstWeek(now);
        for (var index = 0; index < WeekDays.Count; index++)
        {
            WeekDays[index].DayLabel = week[index].DayLabel;
            WeekDays[index].DateLabel = week[index].DateLabel;
            WeekDays[index].IsToday = week[index].IsToday;
        }

        for (var index = 0; index < Timers.Count; index++)
        {
            var state = _timerCalculator.Calculate(_settings.Timers[index], now);
            Timers[index].Update(state);
        }
    }
}
