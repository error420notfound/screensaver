using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using MujiScreenSaver.Interop;

namespace MujiScreenSaver.Services;

public sealed class ScreenSaverExitService
{
    private const double MovementThreshold = 8;
    private readonly Window _window;
    private readonly Action _exit;
    private Point? _initialMousePosition;
    private bool _armed;

    public ScreenSaverExitService(Window window, Action exit)
    {
        _window = window;
        _exit = exit;
    }

    public void Attach()
    {
        _window.Loaded += OnLoaded;
        _window.KeyDown += (_, _) => _exit();
        _window.MouseDown += (_, _) => _exit();
        _window.MouseMove += OnMouseMove;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        _initialMousePosition = NativeMethods.GetCursorPosition();
        _window.Cursor = Cursors.None;
        _armed = true;

        var handle = new WindowInteropHelper(_window).Handle;
        if (handle != IntPtr.Zero)
        {
            NativeMethods.SetForegroundWindow(handle);
        }

        _window.Activate();
        _window.Focus();
    }

    private void OnMouseMove(object sender, MouseEventArgs e)
    {
        if (!_armed || _initialMousePosition is not { } initial)
        {
            return;
        }

        var current = NativeMethods.GetCursorPosition();
        if (Math.Abs(current.X - initial.X) > MovementThreshold || Math.Abs(current.Y - initial.Y) > MovementThreshold)
        {
            _exit();
        }
    }
}
