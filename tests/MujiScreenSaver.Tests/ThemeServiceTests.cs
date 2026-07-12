using System.Windows;
using System.Windows.Media;
using MujiScreenSaver.Models;
using MujiScreenSaver.Services;
using Xunit;

namespace MujiScreenSaver.Tests;

public sealed class ThemeServiceTests
{
    [Fact]
    public void EveryPaletteDefinesLightAndDarkColorRoles()
    {
        Assert.Equal(10, ColorPaletteCatalog.All.Count);

        foreach (var palette in ColorPaletteCatalog.All)
        {
            Assert.False(string.IsNullOrWhiteSpace(palette.Light.Paper.Oklch));
            Assert.False(string.IsNullOrWhiteSpace(palette.Light.Ink.Hex));
            Assert.False(string.IsNullOrWhiteSpace(palette.Dark.Paper.Hex));
            Assert.False(string.IsNullOrWhiteSpace(palette.Dark.Rule.Oklch));
        }
    }

    [Fact]
    public void SystemModeReappliesWhenTheWindowsThemeChanges()
    {
        var resources = CreateBrushResources();
        var systemTheme = new FakeSystemThemeProvider { LightTheme = true };
        using var service = new ThemeService(resources, systemTheme, observeSystemChanges: false);

        service.Apply(ThemeMode.System, ColorPalette.Orange);
        Assert.True(service.IsLightTheme);
        Assert.Equal(Color.FromRgb(255, 244, 240), ((SolidColorBrush)resources["PaperBrush"]).Color);

        systemTheme.LightTheme = false;
        service.ReapplySystemTheme();

        Assert.False(service.IsLightTheme);
        Assert.Equal(Color.FromRgb(58, 19, 7), ((SolidColorBrush)resources["PaperBrush"]).Color);
    }

    [Fact]
    public void ManualThemeOverridesTheWindowsTheme()
    {
        var resources = CreateBrushResources();
        using var service = new ThemeService(resources, new FakeSystemThemeProvider { LightTheme = true }, observeSystemChanges: false);

        service.Apply(ThemeMode.Dark, ColorPalette.Blue);

        Assert.False(service.IsLightTheme);
        Assert.Equal(Color.FromRgb(4, 23, 41), ((SolidColorBrush)resources["PaperBrush"]).Color);
        Assert.Equal(Color.FromRgb(207, 230, 254), ((SolidColorBrush)resources["InkBrush"]).Color);
    }

    [Fact]
    public void UnavailableSystemThemeFallsBackToLight()
    {
        var resources = CreateBrushResources();
        using var service = new ThemeService(resources, new ThrowingSystemThemeProvider(), observeSystemChanges: false);

        service.Apply(ThemeMode.System, ColorPalette.Red);

        Assert.True(service.IsLightTheme);
        Assert.Equal(Color.FromRgb(255, 244, 243), ((SolidColorBrush)resources["PaperBrush"]).Color);
    }

    private static ResourceDictionary CreateBrushResources() => new()
    {
        ["PaperBrush"] = new SolidColorBrush(),
        ["InkBrush"] = new SolidColorBrush(),
        ["MutedBrush"] = new SolidColorBrush(),
        ["RuleBrush"] = new SolidColorBrush()
    };

    private sealed class FakeSystemThemeProvider : ISystemThemeProvider
    {
        public bool LightTheme { get; set; }

        public bool IsLightTheme() => LightTheme;
    }

    private sealed class ThrowingSystemThemeProvider : ISystemThemeProvider
    {
        public bool IsLightTheme() => throw new InvalidOperationException("Registry unavailable");
    }
}
