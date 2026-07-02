using System.Windows;
using MujiScreenSaver.ViewModels;
using MujiScreenSaver.Views;

namespace MujiScreenSaver.Modes;

public sealed class ConfigMode
{
    public int Run(Application app)
    {
        var viewModel = ConfigViewModel.Create();
        var window = new ConfigWindow(viewModel);
        app.ShutdownMode = ShutdownMode.OnMainWindowClose;
        app.MainWindow = window;
        window.Show();
        viewModel.Start();
        app.Run();
        viewModel.Dispose();
        return 0;
    }
}
