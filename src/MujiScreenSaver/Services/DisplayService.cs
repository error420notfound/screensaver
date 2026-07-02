using System.Windows;
using MujiScreenSaver.Interop;

namespace MujiScreenSaver.Services;

public sealed class DisplayService
{
    public IReadOnlyList<MonitorInfo> GetMonitors()
    {
        var monitors = NativeMethods.GetMonitors();
        if (monitors.Count > 0)
        {
            return monitors
                .OrderByDescending(monitor => monitor.IsPrimary)
                .ThenBy(monitor => monitor.Bounds.Left)
                .ThenBy(monitor => monitor.Bounds.Top)
                .ToArray();
        }

        return
        [
            new MonitorInfo(
                new Rect(0, 0, SystemParameters.PrimaryScreenWidth, SystemParameters.PrimaryScreenHeight),
                true)
        ];
    }
}
