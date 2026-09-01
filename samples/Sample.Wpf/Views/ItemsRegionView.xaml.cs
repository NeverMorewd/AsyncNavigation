using AsyncNavigation.Abstractions;
using AsyncNavigation.Floating;
using System.Windows;
using System.Windows.Controls;

namespace Sample.Wpf.Views;

/// <summary>
/// Interaction logic for ItemsRegionView.xaml
/// </summary>
public partial class ItemsRegionView : UserControl,IView
{
    private readonly IViewPlacementService? _placementService;

    public ItemsRegionView() : this(null)
    {
    }

    public ItemsRegionView(IViewPlacementService? placementService)
    {
        _placementService = placementService;
        InitializeComponent();
    }

    private async void FloatSelectedItem_Click(object sender, RoutedEventArgs e)
    {
        if (_placementService is null)
            return;

        try
        {
            await _placementService.FloatAsync("ItemsRegion", options: new FloatingWindowOptions
            {
                Title = "AsyncNavigation floating ItemsRegion item",
                Width = 720,
                Height = 480
            });
        }
        catch (InvalidOperationException ex)
        {
            MessageBox.Show(ex.Message, "Cannot float ItemsRegion", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}
