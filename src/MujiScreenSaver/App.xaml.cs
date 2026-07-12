using System.Windows;
using MujiScreenSaver.Services;

namespace MujiScreenSaver;

public partial class App : Application
{
    public ThemeService ThemeService { get; private set; } = null!;

    public void InitializeThemeService()
    {
        ThemeService = new ThemeService(Resources);
    }

    protected override void OnExit(ExitEventArgs e)
    {
        ThemeService?.Dispose();
        base.OnExit(e);
    }
}
