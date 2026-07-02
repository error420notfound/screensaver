using System.Windows;
using System.Windows.Controls;

namespace MujiScreenSaver.Views;

public partial class ScreenSaverView : UserControl
{
    public static readonly DependencyProperty IsPreviewProperty =
        DependencyProperty.Register(
            nameof(IsPreview),
            typeof(bool),
            typeof(ScreenSaverView),
            new PropertyMetadata(false));

    public ScreenSaverView()
    {
        InitializeComponent();
    }

    public bool IsPreview
    {
        get => (bool)GetValue(IsPreviewProperty);
        set => SetValue(IsPreviewProperty, value);
    }
}
