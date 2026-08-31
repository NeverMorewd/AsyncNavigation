using AsyncNavigation.Abstractions;
using System.Windows.Controls;
using AsyncNavigation.Floating;
using System.Windows;

namespace Sample.Wpf.Views;

/// <summary>
/// Interaction logic for TabRegionView.xaml
/// </summary>
public partial class TabRegionView : UserControl, IView
{
    private readonly IViewPlacementService? _placementService;

    public TabRegionView() : this(null)
    {
    }

    public TabRegionView(IViewPlacementService? placementService)
    {
        _placementService = placementService;
        InitializeComponent();
    }

    private async void FloatCurrent_Click(object sender, RoutedEventArgs e)
    {
        if (_placementService is null)
            return;

        try
        {
            await _placementService.FloatAsync("TabRegion", options: new FloatingWindowOptions
            {
                Title = "AsyncNavigation floating tab",
                Width = 720,
                Height = 480
            });
        }
        catch (InvalidOperationException ex)
        {
            MessageBox.Show(ex.Message, "Cannot float tab", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}
