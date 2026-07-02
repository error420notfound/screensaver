using System.Runtime.InteropServices;
using System.Windows;

namespace MujiScreenSaver.Interop;

public static class NativeMethods
{
    public const int WsChild = 0x40000000;
    public const int WsVisible = 0x10000000;

    private const int MonitorDefaultToNearest = 2;

    public delegate bool MonitorEnumProc(IntPtr hMonitor, IntPtr hdcMonitor, ref RectNative lprcMonitor, IntPtr dwData);

    [DllImport("user32.dll")]
    private static extern bool EnumDisplayMonitors(IntPtr hdc, IntPtr lprcClip, MonitorEnumProc lpfnEnum, IntPtr dwData);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool GetMonitorInfo(IntPtr hMonitor, ref MonitorInfoNative lpmi);

    [DllImport("user32.dll")]
    public static extern bool GetClientRect(IntPtr hWnd, out RectNative lpRect);

    [DllImport("user32.dll")]
    public static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int x, int y, int cx, int cy, uint uFlags);

    [DllImport("user32.dll")]
    public static extern bool SetForegroundWindow(IntPtr hWnd);

    [DllImport("user32.dll")]
    public static extern bool IsWindow(IntPtr hWnd);

    [DllImport("user32.dll")]
    private static extern bool GetCursorPos(out PointNative lpPoint);

    [DllImport("user32.dll")]
    private static extern IntPtr MonitorFromWindow(IntPtr hwnd, uint dwFlags);

    [DllImport("shcore.dll")]
    private static extern int GetDpiForMonitor(IntPtr hmonitor, MonitorDpiType dpiType, out uint dpiX, out uint dpiY);

    public static IReadOnlyList<MonitorInfo> GetMonitors()
    {
        var monitors = new List<MonitorInfo>();
        EnumDisplayMonitors(IntPtr.Zero, IntPtr.Zero, (IntPtr monitor, IntPtr hdc, ref RectNative bounds, IntPtr data) =>
        {
            var info = new MonitorInfoNative();
            info.Size = Marshal.SizeOf<MonitorInfoNative>();

            if (GetMonitorInfo(monitor, ref info))
            {
                var scale = GetMonitorScale(monitor);
                monitors.Add(new MonitorInfo(
                    new Rect(
                        info.Monitor.Left / scale,
                        info.Monitor.Top / scale,
                        info.Monitor.Width / scale,
                        info.Monitor.Height / scale),
                    (info.Flags & 1) == 1));
            }

            return true;
        }, IntPtr.Zero);

        return monitors;
    }

    public static Point GetCursorPosition()
    {
        return GetCursorPos(out var point)
            ? new Point(point.X, point.Y)
            : new Point(0, 0);
    }

    public static double GetWindowScale(IntPtr hwnd)
    {
        var monitor = MonitorFromWindow(hwnd, MonitorDefaultToNearest);
        return GetMonitorScale(monitor);
    }

    private static double GetMonitorScale(IntPtr monitor)
    {
        if (monitor != IntPtr.Zero && GetDpiForMonitor(monitor, MonitorDpiType.EffectiveDpi, out var dpiX, out _) == 0 && dpiX > 0)
        {
            return dpiX / 96.0;
        }

        return 1.0;
    }

    public enum MonitorDpiType
    {
        EffectiveDpi = 0
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct RectNative
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;

        public int Width => Right - Left;

        public int Height => Bottom - Top;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct MonitorInfoNative
    {
        public int Size;
        public RectNative Monitor;
        public RectNative WorkArea;
        public uint Flags;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct PointNative
    {
        public int X;
        public int Y;
    }
}
