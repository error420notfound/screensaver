using System.Text.Json;
using System.Text.Json.Serialization;
using MujiScreenSaver.Models;
using MujiScreenSaver.Services;
using Xunit;

namespace MujiScreenSaver.Tests;

public sealed class TimerStoreTests
{
    [Fact]
    public void MissingFileLoadsDefaultThreeTimers()
    {
        using var directory = new TemporaryDirectory();
        var store = new TimerStore(new TimerStateCalculator(), Path.Combine(directory.Path, "timers.json"));

        var file = store.Load();

        Assert.Equal(3, file.Timers.Count);
        Assert.All(file.Timers, timer => Assert.False(timer.IsVisible));
        Assert.True(file.Use12HourTime);
    }

    [Fact]
    public void SaveWritesReadableNormalizedJson()
    {
        using var directory = new TemporaryDirectory();
        var path = Path.Combine(directory.Path, "timers.json");
        var store = new TimerStore(new TimerStateCalculator(), path);

        store.Save(new TimerSettingsFile
        {
            Timers =
            [
                new TimerDefinition
                {
                    Id = "focus",
                    Label = "Focus",
                    DurationSeconds = 3600,
                    IsVisible = true
                }
            ],
            Use12HourTime = false,
            ThemeMode = ThemeMode.Dark,
            ColorPalette = ColorPalette.Blue
        });

        var json = File.ReadAllText(path);
        var file = JsonSerializer.Deserialize<TimerSettingsFile>(json, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            Converters = { new JsonStringEnumConverter() }
        });

        Assert.NotNull(file);
        Assert.Equal(3, file!.Timers.Count);
        Assert.Equal("focus", file.Timers[0].Id);
        Assert.False(file.Use12HourTime);
        Assert.Equal(ThemeMode.Dark, file.ThemeMode);
        Assert.Equal(ColorPalette.Blue, file.ColorPalette);
    }

    [Fact]
    public void MissingAppearanceSettingsMigrateToSystemOrange()
    {
        using var directory = new TemporaryDirectory();
        var path = Path.Combine(directory.Path, "timers.json");
        File.WriteAllText(path, "{\"version\":1,\"timers\":[]}");
        var store = new TimerStore(new TimerStateCalculator(), path);

        var file = store.Load();

        Assert.Equal(2, file.Version);
        Assert.Equal(ThemeMode.System, file.ThemeMode);
        Assert.Equal(ColorPalette.Orange, file.ColorPalette);
    }

    [Fact]
    public void InvalidAppearanceSettingsFallBackWithoutDiscardingTimers()
    {
        using var directory = new TemporaryDirectory();
        var path = Path.Combine(directory.Path, "timers.json");
        File.WriteAllText(path, "{\"version\":2,\"themeMode\":\"Nope\",\"colorPalette\":\"Unknown\",\"timers\":[{\"id\":\"focus\",\"label\":\"Focus\",\"durationSeconds\":60,\"isVisible\":true}]}");
        var store = new TimerStore(new TimerStateCalculator(), path);

        var file = store.Load();

        Assert.Equal(ThemeMode.System, file.ThemeMode);
        Assert.Equal(ColorPalette.Orange, file.ColorPalette);
        Assert.Equal("focus", file.Timers[0].Id);
    }

    [Fact]
    public void CorruptJsonIsPreservedAndDefaultsAreLoaded()
    {
        using var directory = new TemporaryDirectory();
        var path = Path.Combine(directory.Path, "timers.json");
        File.WriteAllText(path, "{ bad json");
        var store = new TimerStore(new TimerStateCalculator(), path);

        var file = store.Load();

        Assert.Equal(3, file.Timers.Count);
        Assert.False(File.Exists(path));
        Assert.Single(Directory.GetFiles(directory.Path, "timers.corrupt.*.json"));
    }

    private sealed class TemporaryDirectory : IDisposable
    {
        public TemporaryDirectory()
        {
            Path = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "MujiScreenSaverTests", Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(Path);
        }

        public string Path { get; }

        public void Dispose()
        {
            if (Directory.Exists(Path))
            {
                Directory.Delete(Path, recursive: true);
            }
        }
    }
}
