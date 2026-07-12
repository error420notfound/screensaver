using Microsoft.Win32;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;
using MujiScreenSaver.Models;

namespace MujiScreenSaver.Services;

public interface ISystemThemeProvider
{
    bool IsLightTheme();
}

public sealed class WindowsSystemThemeProvider : ISystemThemeProvider
{
    private const string PersonalizeKey = @"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize";

    public bool IsLightTheme()
    {
        try
        {
            using var key = Registry.CurrentUser.OpenSubKey(PersonalizeKey);
            var value = key?.GetValue("AppsUseLightTheme");
            return value is int number ? number != 0 : true;
        }
        catch
        {
            return true;
        }
    }
}

public sealed class ThemeService : IDisposable
{
    private readonly ResourceDictionary _resources;
    private readonly ISystemThemeProvider _systemThemeProvider;
    private readonly Dispatcher _dispatcher;
    private readonly bool _observeSystemChanges;
    private ThemeMode _themeMode = ThemeMode.System;
    private ColorPalette _colorPalette = ColorPalette.Orange;
    private bool _disposed;

    public ThemeService(
        ResourceDictionary resources,
        ISystemThemeProvider? systemThemeProvider = null,
        bool observeSystemChanges = true)
    {
        _resources = resources;
        _systemThemeProvider = systemThemeProvider ?? new WindowsSystemThemeProvider();
        _dispatcher = Dispatcher.CurrentDispatcher;
        _observeSystemChanges = observeSystemChanges;

        if (_observeSystemChanges)
        {
            SystemEvents.UserPreferenceChanged += OnUserPreferenceChanged;
        }
    }

    public bool IsLightTheme { get; private set; } = true;

    public void Apply(ThemeMode themeMode, ColorPalette colorPalette)
    {
        _themeMode = Enum.IsDefined(themeMode) ? themeMode : ThemeMode.System;
        _colorPalette = Enum.IsDefined(colorPalette) ? colorPalette : ColorPalette.Orange;
        ApplyCurrentTheme();
    }

    public void ReapplySystemTheme()
    {
        if (_themeMode == ThemeMode.System)
        {
            ApplyCurrentTheme();
        }
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        if (_observeSystemChanges)
        {
            SystemEvents.UserPreferenceChanged -= OnUserPreferenceChanged;
        }
    }

    private void ApplyCurrentTheme()
    {
        IsLightTheme = _themeMode switch
        {
            ThemeMode.Light => true,
            ThemeMode.Dark => false,
            _ => GetSystemThemeOrLight()
        };

        var colors = ColorPaletteCatalog.Get(_colorPalette).For(IsLightTheme);
        ReplaceBrush("PaperBrush", colors.Paper.Hex);
        ReplaceBrush("InkBrush", colors.Ink.Hex);
        ReplaceBrush("MutedBrush", colors.Muted.Hex);
        ReplaceBrush("RuleBrush", colors.Rule.Hex);
    }

    private void OnUserPreferenceChanged(object? sender, UserPreferenceChangedEventArgs e)
    {
        if (_themeMode != ThemeMode.System || _disposed)
        {
            return;
        }

        _dispatcher.BeginInvoke(ReapplySystemTheme);
    }

    private void ReplaceBrush(string resourceKey, string hex)
    {
        _resources[resourceKey] = new SolidColorBrush((Color)ColorConverter.ConvertFromString(hex));
    }

    private bool GetSystemThemeOrLight()
    {
        try
        {
            return _systemThemeProvider.IsLightTheme();
        }
        catch
        {
            return true;
        }
    }
}
