using AsyncNavigation.Abstractions;
using AsyncNavigation.Floating;
using System.Windows;
using System.Windows.Controls;

namespace Sample.Wpf.Views;

public partial class ListBoxRegionView : UserControl, IView
{
    private readonly IViewPlacementService? _placementService;

    public ListBoxRegionView() : this(null)
    {
    }

    public ListBoxRegionView(IViewPlacementService? placementService)
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
            await _placementService.FloatAsync("CustomListBoxRegion", options: new FloatingWindowOptions
            {
                Title = "AsyncNavigation floating CustomListBoxRegion item",
                Width = 720,
                Height = 480
            });
        }
        catch (Exception ex) when (ex is InvalidOperationException or NotSupportedException)
        {
            MessageBox.Show(ex.Message, "Cannot float CustomListBoxRegion", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}
