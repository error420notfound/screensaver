using MujiScreenSaver.ViewModels;
using Xunit;

namespace MujiScreenSaver.Tests;

public sealed class ScreenSaverViewModelTests
{
    [Fact]
    public void FormatTimeUses12HourClockWithMeridiem()
    {
        var now = new DateTimeOffset(2026, 7, 12, 21, 5, 7, TimeSpan.FromHours(5.5));

        var text = ScreenSaverViewModel.FormatTime(now, use12HourTime: true, isPreview: false);

        Assert.Equal("9:05:07 PM", text);
    }

    [Fact]
    public void FormatTimeCanUse24HourClock()
    {
        var now = new DateTimeOffset(2026, 7, 12, 21, 5, 7, TimeSpan.FromHours(5.5));

        var text = ScreenSaverViewModel.FormatTime(now, use12HourTime: false, isPreview: true);

        Assert.Equal("21:05", text);
    }
}
