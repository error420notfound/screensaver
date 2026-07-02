using System.Text.Json;
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
            ]
        });

        var json = File.ReadAllText(path);
        var file = JsonSerializer.Deserialize<TimerSettingsFile>(json, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        Assert.NotNull(file);
        Assert.Equal(3, file!.Timers.Count);
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
