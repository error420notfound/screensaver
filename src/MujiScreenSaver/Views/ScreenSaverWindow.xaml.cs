using System.Windows;
using MujiScreenSaver.ViewModels;

namespace MujiScreenSaver.Views;

public partial class ScreenSaverWindow : Window
{
    public ScreenSaverWindow(ScreenSaverViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
