using System.Collections.ObjectModel;
using System.Windows.Threading;
using MujiScreenSaver.Models;
using MujiScreenSaver.Services;

namespace MujiScreenSaver.ViewModels;

public sealed class ConfigViewModel : ViewModelBase, IDisposable
{
    private readonly ClockService _clock;
    private readonly TimerStore _store;
    private readonly TimerStateCalculator _calculator;
    private readonly DispatcherTimer _refreshTimer;
    private bool _disposed;
    private string _storagePath = string.Empty;

    private ConfigViewModel(ClockService clock, TimerStore store, TimerStateCalculator calculator)
    {
        _clock = clock;
        _store = store;
        _calculator = calculator;
        StoragePath = _store.FilePath;
        _refreshTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
        _refreshTimer.Tick += (_, _) => Refresh();
        Load();
    }

    public ObservableCollection<ConfigTimerViewModel> Timers { get; } = [];

    public string StoragePath
    {
        get => _storagePath;
        private set => SetProperty(ref _storagePath, value);
    }

    public static ConfigViewModel Create()
    {
        var calculator = new TimerStateCalculator();
        return new ConfigViewModel(new ClockService(), new TimerStore(calculator), calculator);
    }

    public void Start()
    {
        _refreshTimer.Start();
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        _refreshTimer.Stop();
    }

    private void Load()
    {
        Timers.Clear();
        var file = _store.Load();
        foreach (var timer in file.Timers)
        {
            Timers.Add(new ConfigTimerViewModel(timer, _clock.Now, _calculator, Save));
        }
    }

    private void Save()
    {
        var file = new TimerSettingsFile
        {
            Version = 1,
            Timers = Timers.Select(timer => timer.ToDefinition()).ToList()
        };
        _store.Save(file);
    }

    private void Refresh()
    {
        foreach (var timer in Timers)
        {
            timer.RefreshStatus();
        }
    }
}
