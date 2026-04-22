using AsyncNavigation;
using AsyncNavigation.Avalonia;
using AsyncNavigation.Core;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
using Sample.Avalonia.Regions;
using Sample.Avalonia.Views;
using Sample.Common;

namespace Sample.Avalonia;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }
    public override void OnFrameworkInitializationCompleted()
    {

        base.OnFrameworkInitializationCompleted();
        NavigationOptions navigationOptions = new()
        {
            /// default is CancelCurrent <see cref="NavigationJobStrategy.CancelCurrent"/>
            NavigationJobStrategy = NavigationJobStrategy.CancelCurrent
        };

        var services = new ServiceCollection();
        services.AddNavigationSupport(navigationOptions)
                .AddSingletonWithAllMembers<MainWindowViewModel>()
                .RegisterView<AView, AViewModel>(nameof(AView))
                .RegisterView<BView, BViewModel>(nameof(BView))
                .RegisterView<CView, CViewModel>(nameof(CView))
                .RegisterView<DView, DViewModel>(nameof(DView))
                .RegisterView<HeavyView, HeavyViewModel>(nameof(HeavyView))
                .RegisterView<NavigationPageView, AViewModel>(nameof(NavigationPageView))
                .RegisterView<TabbedPageView, AViewModel>(nameof(TabbedPageView))
                .RegisterView<ListBoxRegionView, ListBoxRegionViewModel>(nameof(ListBoxRegionView))
                .RegisterDialogWindow<AWindow, AViewModel>(nameof(AWindow))
                .RegisterRegionIndicatorProvider<NotifyIndicatorProvider>()
                .RegisterInnerIndicatorProvider<InnerIndicatorProvider>()
                .RegisterRegionAdapter<ListBoxRegionAdapter>()
                .RegisterRouter((mapper, sp) =>
                {
                    mapper.MapNavigation("Path_ChildHeavyView", 
                                         new NavigationTarget("MainRegion", "CView"),
                                         new NavigationTarget("ChildContentRegion", "HeavyView"));

                    mapper.MapNavigation("Path_ChildAView",
                                         new NavigationTarget("MainRegion", "CView"),
                                         new NavigationTarget("ChildContentRegion", "AView"));

                    mapper.MapNavigation("Path_TabHeavyView", new NavigationTarget("MainRegion", "DView"),
                                       new NavigationTarget("TabRegion", "HeavyView"));

                    mapper.MapNavigation("Tab.Tab_A",
                                         new NavigationTarget("MainRegion", "DView"),
                                         new NavigationTarget("TabRegion", "AView"))
                              .WithSegments("Tab","Tab_A");

                    mapper.MapNavigation("Path_UnknownView", new NavigationTarget("UnknownRegion", "UnknownView"))
                          .WithFallback(new NavigationTarget("MainRegion", "AView"));
                });
        var sp = services.BuildServiceProvider();

        var converter = sp.GetRequiredService<IconDescriptorConverter>();
        Resources[nameof(IconDescriptorConverter)] = converter;

        #region setup lifetime


        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow
            {
                DataContext = sp.GetRequiredService<MainWindowViewModel>()
            };
            desktop.ShutdownMode = ShutdownMode.OnMainWindowClose;
        }
        else if (ApplicationLifetime is ISingleViewApplicationLifetime singleViewPlatform)
        {
            singleViewPlatform.MainView = new MainView
            {
                DataContext = sp.GetRequiredService<MainWindowViewModel>()
            };
        }
        #endregion
    }
}