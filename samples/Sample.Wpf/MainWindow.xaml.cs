using System.Windows;

using AsyncNavigation.Floating;

namespace Sample.Wpf;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private readonly IViewPlacementService? _placementService;

    public MainWindow() : this(null)
    {
    }

    public MainWindow(IViewPlacementService? placementService)
    {
        _placementService = placementService;
        InitializeComponent();
    }

    private async void FloatMainRegion_Click(object sender, RoutedEventArgs e)
    {
        if (_placementService is null)
            return;

        try
        {
            await _placementService.FloatAsync("MainRegion", options: new FloatingWindowOptions
            {
                Title = "AsyncNavigation floating MainRegion",
                Width = 900,
                Height = 600
            });
        }
        catch (InvalidOperationException ex)
        {
            MessageBox.Show(ex.Message, "Cannot float MainRegion", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}
