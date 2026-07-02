using System.Windows;
using MujiScreenSaver.ViewModels;

namespace MujiScreenSaver.Views;

public partial class ConfigWindow : Window
{
    public ConfigWindow(ConfigViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
