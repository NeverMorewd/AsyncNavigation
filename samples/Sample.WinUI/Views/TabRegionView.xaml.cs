using AsyncNavigation.Abstractions;
using Microsoft.UI.Xaml.Controls;
using AsyncNavigation;
using Sample.Common;
namespace Sample.WinUI.Views;
public sealed partial class TabRegionView : UserControl, IView
{
    public TabRegionView() => InitializeComponent();

    private async void OnTabCloseRequested(TabView sender, TabViewTabCloseRequestedEventArgs args)
    {
        if (args.Item is NavigationContext context && context.TryResolveNavigationAware(out var aware) && aware is ViewModelBase viewModel)
            await viewModel.RequestUnloadAsync(CancellationToken.None);
    }
}
