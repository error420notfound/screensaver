using System.Globalization;

namespace MujiScreenSaver.Services;

public sealed class WeekCalendarService
{
    private static readonly string[] DayLabels = ["M", "T", "W", "T", "F", "S", "S"];

    public string FormatDate(DateTimeOffset now)
    {
        return now.LocalDateTime.ToString("dddd, dd MMMM yyyy", CultureInfo.InvariantCulture).ToUpperInvariant();
    }

    public IReadOnlyList<WeekDayInfo> GetMondayFirstWeek(DateTimeOffset now)
    {
        var today = DateOnly.FromDateTime(now.LocalDateTime);
        var offset = ((int)today.DayOfWeek + 6) % 7;
        var monday = today.AddDays(-offset);

        return Enumerable.Range(0, 7)
            .Select(index =>
            {
                var date = monday.AddDays(index);
                return new WeekDayInfo(DayLabels[index], date.Day.ToString("00", CultureInfo.InvariantCulture), date == today);
            })
            .ToArray();
    }
}

public sealed record WeekDayInfo(string DayLabel, string DateLabel, bool IsToday);
