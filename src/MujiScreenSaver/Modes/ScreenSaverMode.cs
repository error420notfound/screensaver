using System.Windows;
using MujiScreenSaver.Services;
using MujiScreenSaver.ViewModels;
using MujiScreenSaver.Views;

namespace MujiScreenSaver.Modes;

public sealed class ScreenSaverMode
{
    private readonly List<ScreenSaverWindow> _windows = [];
    private bool _isExiting;

    public int Run(Application app)
    {
        var displayService = new DisplayService();
        var monitors = displayService.GetMonitors();

        foreach (var monitor in monitors)
        {
            var viewModel = ScreenSaverViewModel.Create(isPreview: false);
            var window = new ScreenSaverWindow(viewModel)
            {
                Left = monitor.Bounds.Left,
                Top = monitor.Bounds.Top,
                Width = monitor.Bounds.Width,
                Height = monitor.Bounds.Height,
                Topmost = true,
                ShowInTaskbar = false
            };

            new ScreenSaverExitService(window, ExitAll).Attach();
            _windows.Add(window);
            window.Show();
            viewModel.Start();
        }

        if (_windows.Count == 0)
        {
            return 0;
        }

        app.Deactivated += (_, _) => ExitAll();

        app.Run();
        return 0;
    }

    private void ExitAll()
    {
        if (_isExiting)
        {
            return;
        }

        _isExiting = true;

        foreach (var window in _windows.ToArray())
        {
            if (window.DataContext is IDisposable disposable)
            {
                disposable.Dispose();
            }

            window.Close();
        }

        Application.Current.Shutdown();
    }
}
