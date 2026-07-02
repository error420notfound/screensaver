using System.Windows.Threading;
using System.Windows.Interop;

namespace MujiScreenSaver.Interop;

public sealed class PreviewHost : IDisposable
{
    private readonly IntPtr _parentHandle;
    private readonly HwndSource _source;
    private readonly DispatcherTimer _resizeTimer;
    private bool _disposed;

    public PreviewHost(IntPtr parentHandle, System.Windows.Media.Visual rootVisual)
    {
        _parentHandle = parentHandle;

        var size = GetClientSize();
        var parameters = new HwndSourceParameters("MujiScreenSaverPreview")
        {
            ParentWindow = parentHandle,
            WindowStyle = NativeMethods.WsChild | NativeMethods.WsVisible,
            Width = size.Width,
            Height = size.Height
        };

        _source = new HwndSource(parameters)
        {
            RootVisual = rootVisual
        };

        _resizeTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(500)
        };
        _resizeTimer.Tick += (_, _) => ResizeToParent();
        _resizeTimer.Start();
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        _resizeTimer.Stop();
        _source.Dispose();
    }

    private void ResizeToParent()
    {
        if (!NativeMethods.IsWindow(_parentHandle))
        {
            System.Windows.Application.Current.Shutdown();
            return;
        }

        var size = GetClientSize();
        NativeMethods.SetWindowPos(_source.Handle, IntPtr.Zero, 0, 0, size.Width, size.Height, 0);
    }

    private (int Width, int Height) GetClientSize()
    {
        return NativeMethods.GetClientRect(_parentHandle, out var rect)
            ? (Math.Max(1, rect.Width), Math.Max(1, rect.Height))
            : (320, 200);
    }
}
