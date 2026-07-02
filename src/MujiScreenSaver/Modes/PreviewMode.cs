using System.Windows;
using MujiScreenSaver.Interop;
using MujiScreenSaver.ViewModels;
using MujiScreenSaver.Views;

namespace MujiScreenSaver.Modes;

public sealed class PreviewMode
{
    private readonly IntPtr _parentHandle;

    public PreviewMode(IntPtr parentHandle)
    {
        _parentHandle = parentHandle;
    }

    public int Run(Application app)
    {
        var viewModel = ScreenSaverViewModel.Create(isPreview: true);
        var view = new ScreenSaverView
        {
            DataContext = viewModel,
            IsPreview = true
        };

        using var host = new PreviewHost(_parentHandle, view);
        viewModel.Start();
        app.Run();
        viewModel.Dispose();
        return 0;
    }
}
