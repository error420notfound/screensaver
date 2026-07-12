using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Threading;
using MujiScreenSaver.Models;
using MujiScreenSaver.Services;

namespace MujiScreenSaver.ViewModels;

public sealed class ConfigViewModel : ViewModelBase, IDisposable
{
    private readonly ClockService _clock;
    private readonly TimerStore _store;
    private readonly TimerStateCalculator _calculator;
    private readonly ThemeService? _themeService;
    private readonly DispatcherTimer _refreshTimer;
    private bool _disposed;
    private bool _use12HourTime = true;
    private ThemeMode _themeMode = ThemeMode.System;
    private ColorPalette _colorPalette = ColorPalette.Orange;
    private string _storagePath = string.Empty;

    private ConfigViewModel(ClockService clock, TimerStore store, TimerStateCalculator calculator, ThemeService? themeService)
    {
        _clock = clock;
        _store = store;
        _calculator = calculator;
        _themeService = themeService;
        StoragePath = _store.FilePath;
        _refreshTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
        _refreshTimer.Tick += (_, _) => Refresh();
        Load();
    }

    public ObservableCollection<ConfigTimerViewModel> Timers { get; } = [];

    public bool Use12HourTime
    {
        get => _use12HourTime;
        set
        {
            if (SetProperty(ref _use12HourTime, value))
            {
                Save();
            }
        }
    }

    public IReadOnlyList<ThemeMode> ThemeModes { get; } = Enum.GetValues<ThemeMode>();

    public IReadOnlyList<ColorPaletteDefinition> PaletteOptions => ColorPaletteCatalog.All;

    public ThemeMode ThemeMode
    {
        get => _themeMode;
        set
        {
            if (SetProperty(ref _themeMode, value))
            {
                Save();
                ApplyAppearance();
            }
        }
    }

    public ColorPalette ColorPalette
    {
        get => _colorPalette;
        set
        {
            if (SetProperty(ref _colorPalette, value))
            {
                Save();
                ApplyAppearance();
            }
        }
    }

    public string StoragePath
    {
        get => _storagePath;
        private set => SetProperty(ref _storagePath, value);
    }

    public static ConfigViewModel Create()
    {
        var calculator = new TimerStateCalculator();
        return new ConfigViewModel(
            new ClockService(),
            new TimerStore(calculator),
            calculator,
            (Application.Current as App)?.ThemeService);
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
        _use12HourTime = file.Use12HourTime;
        _themeMode = file.ThemeMode;
        _colorPalette = file.ColorPalette;
        OnPropertyChanged(nameof(Use12HourTime));
        OnPropertyChanged(nameof(ThemeMode));
        OnPropertyChanged(nameof(ColorPalette));
        ApplyAppearance();

        foreach (var timer in file.Timers)
        {
            Timers.Add(new ConfigTimerViewModel(timer, _clock.Now, _calculator, Save));
        }
    }

    private void Save()
    {
        var file = new TimerSettingsFile
        {
            Version = 2,
            Use12HourTime = Use12HourTime,
            ThemeMode = ThemeMode,
            ColorPalette = ColorPalette,
            Timers = Timers.Select(timer => timer.ToDefinition()).ToList()
        };
        _store.Save(file);
    }

    private void ApplyAppearance() => _themeService?.Apply(ThemeMode, ColorPalette);

    private void Refresh()
    {
        foreach (var timer in Timers)
        {
            timer.RefreshStatus();
        }
    }
}
