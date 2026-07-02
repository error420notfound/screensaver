using MujiScreenSaver.Services;
using Xunit;

namespace MujiScreenSaver.Tests;

public sealed class WeekCalendarServiceTests
{
    private readonly WeekCalendarService _service = new();

    [Fact]
    public void WeekStartsOnMondayAcrossMonthBoundary()
    {
        var now = new DateTimeOffset(2026, 7, 2, 12, 0, 0, TimeSpan.FromHours(5.5));

        var week = _service.GetMondayFirstWeek(now);

        Assert.Equal(["29", "30", "01", "02", "03", "04", "05"], week.Select(day => day.DateLabel));
        Assert.Equal(3, week.ToList().FindIndex(day => day.IsToday));
    }

    [Fact]
    public void DateFormatIsUppercaseInvariant()
    {
        var now = new DateTimeOffset(2026, 7, 2, 12, 0, 0, TimeSpan.FromHours(5.5));

        Assert.Equal("THURSDAY, 02 JULY 2026", _service.FormatDate(now));
    }
}
