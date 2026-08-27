using AsyncNavigation;
using AsyncNavigation.Core;
using AsyncNavigation.WinUI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using ReactiveUI.Builder;
using Sample.Common;
using Sample.WinUI.Views;
using Sample.WinUI.Regions;

namespace Sample.WinUI;

public partial class App : Application
{
    private Window? _window;

    public App()
    {
        InitializeComponent();
    }

    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        RxAppBuilder.CreateReactiveUIBuilder()
            .WithWinUI()
            .BuildApp();
        var services = new ServiceCollection();
        services.AddNavigationSupport()
            .AddSingleton<MainWindowViewModel>()
            .RegisterRegionAdapter<ListBoxRegionAdapter>()
            .RegisterView<LightView, LightViewModel>(nameof(LightView))
            .RegisterView<ItemsRegionView, ItemsRegionViewModel>(nameof(ItemsRegionView))
            .RegisterView<ChildContentRegionView, ChildContentRegionViewModel>(nameof(ChildContentRegionView))
            .RegisterView<TabRegionView, TabRegionViewModel>(nameof(TabRegionView))
            .RegisterView<NavigationViewRegionView, LightViewModel>(nameof(NavigationViewRegionView))
            .RegisterView<HeavyView, HeavyViewModel>(nameof(HeavyView))
            .RegisterDialogWindow<AWindow, LightViewModel>(nameof(AWindow))
            .RegisterInnerIndicatorProvider<InnerIndicatorProvider>()
            .RegisterRegionIndicatorProvider<MessageBoxIndicatorProvider>()
            .RegisterView<ListBoxRegionView, ListBoxRegionViewModel>(nameof(ListBoxRegionView))
            .RegisterRouter((mapper, _) =>
            {
                mapper.MapNavigation("Path_ChildHeavyView",
                    new NavigationTarget("MainRegion", nameof(ChildContentRegionView)),
                    new NavigationTarget("ChildContentRegion", nameof(HeavyView)));
                mapper.MapNavigation("Path_ChildAView",
                    new NavigationTarget("MainRegion", nameof(ChildContentRegionView)),
                    new NavigationTarget("ChildContentRegion", nameof(LightView)));
                mapper.MapNavigation("Path_TabHeavyView",
                    new NavigationTarget("MainRegion", nameof(TabRegionView)),
                    new NavigationTarget("TabRegion", nameof(HeavyView)));
                mapper.MapNavigation("Tab.Tab_A",
                    new NavigationTarget("MainRegion", nameof(TabRegionView)),
                    new NavigationTarget("TabRegion", nameof(LightView)))
                    .WithSegments("Tab", "Tab_A");
                mapper.MapNavigation("Path_UnknownView", new NavigationTarget("UnknownRegion", "UnknownView"))
                    .WithFallback(new NavigationTarget("MainRegion", nameof(LightView)));
            });

        var serviceProvider = services.BuildServiceProvider();
        Resources[nameof(IconDescriptorConverter)] = serviceProvider.GetRequiredService<IconDescriptorConverter>();

        _window = new MainWindow
        {
            Content = new MainPage
            {
                DataContext = serviceProvider.GetRequiredService<MainWindowViewModel>()
            }
        };
        _window.Title = "AsyncNavigation · WinUI 3 Sample";
        _window.AppWindow.Resize(new Windows.Graphics.SizeInt32(1100, 720));
        _window.Activate();
    }
}
