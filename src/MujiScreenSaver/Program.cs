using System.Windows;
using MujiScreenSaver.Modes;

namespace MujiScreenSaver;

public static class Program
{
    [STAThread]
    public static int Main(string[] args)
    {
        var options = CommandLineOptions.Parse(args);
        var app = new App
        {
            ShutdownMode = ShutdownMode.OnExplicitShutdown
        };
        app.InitializeComponent();

        try
        {
            return options.Mode switch
            {
                ScreenSaverLaunchMode.FullScreen => new ScreenSaverMode().Run(app),
                ScreenSaverLaunchMode.Preview => new PreviewMode(options.PreviewHandle!.Value).Run(app),
                ScreenSaverLaunchMode.Configure => new ConfigMode().Run(app),
                _ => new ConfigMode().Run(app)
            };
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                ex.Message,
                "Muji Screen Saver",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
            return 1;
        }
    }
}
