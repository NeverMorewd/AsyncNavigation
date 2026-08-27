using AsyncNavigation.Abstractions;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Sample.WinUI.Views;

public sealed partial class NavigationViewRegionView : UserControl, IView
{
    private readonly IRegionManager _regionManager;

    public NavigationViewRegionView(IRegionManager regionManager)
    {
        _regionManager = regionManager;
        InitializeComponent();
        Loaded += OnLoaded;
    }

    private async void OnLoaded(object sender, RoutedEventArgs e)
    {
        Loaded -= OnLoaded;
        await _regionManager.RequestNavigateAsync("NavigationViewRegion", "LightView");
    }
}
