using AsyncNavigation.Abstractions;
using AsyncNavigation.Floating;
using System.Windows;
using System.Windows.Controls;

namespace Sample.Wpf.Views;

public partial class ChildContentRegionView : UserControl, IView
{
    private readonly IViewPlacementService? _placementService;

    public ChildContentRegionView() : this(null)
    {
    }

    public ChildContentRegionView(IViewPlacementService? placementService)
    {
        _placementService = placementService;
        InitializeComponent();
    }

    private async void FloatChildRegion_Click(object sender, RoutedEventArgs e)
    {
        if (_placementService is null)
            return;

        try
        {
            await _placementService.FloatAsync("ChildContentRegion", options: new FloatingWindowOptions
            {
                Title = "AsyncNavigation floating ChildContentRegion",
                Width = 720,
                Height = 480
            });
        }
        catch (Exception ex) when (ex is InvalidOperationException or NotSupportedException)
        {
            MessageBox.Show(ex.Message, "Cannot float ChildContentRegion", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}
