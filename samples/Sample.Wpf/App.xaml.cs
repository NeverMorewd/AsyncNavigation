using AsyncNavigation.Core;
using AsyncNavigation.Wpf;
using Microsoft.Extensions.DependencyInjection;
using ReactiveUI;
using Sample.Avalonia;
using Sample.Common;
using Sample.Wpf.Regions;
using Sample.Wpf.Views;
using System.Windows;

namespace Sample.Wpf
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            Current.ShutdownMode = ShutdownMode.OnMainWindowClose;
            PlatformRegistrationManager.SetRegistrationNamespaces(RegistrationNamespace.Wpf);


            var services = new ServiceCollection();
            services.AddNavigationSupport()
                .AddSingleton<MainWindowViewModel>()
                .RegisterRegionAdapter<ListBoxRegionAdapter>()
                .RegisterView<LightView, LightViewModel>(nameof(LightView))
                .RegisterView<ItemsRegionView, ItemsRegionViewModel>(nameof(ItemsRegionView))
                .RegisterView<ChildContentRegionView, ChildContentRegionViewModel>(nameof(ChildContentRegionView))
                .RegisterView<TabRegionView, TabRegionViewModel>(nameof(TabRegionView))
                .RegisterView<HeavyView, HeavyViewModel>(nameof(HeavyView))
                .RegisterDialogWindow<AWindow, LightViewModel>(nameof(AWindow))
                .RegisterInnerIndicatorProvider<InnerIndicatorProvider>()
                .RegisterRegionIndicatorProvider<MessageBoxIndicatorProvider>()
                .RegisterView<ListBoxRegionView, ListBoxRegionViewModel>(nameof(ListBoxRegionView))
                .RegisterRouter((mapper, sp) =>
                {
                    mapper.MapNavigation("Path_ChildHeavyView",
                                         new NavigationTarget("MainRegion", "ChildContentRegionView"),
                                         new NavigationTarget("ChildContentRegion", "HeavyView"));

                    mapper.MapNavigation("Path_ChildAView",
                                         new NavigationTarget("MainRegion", "ChildContentRegionView"),
                                         new NavigationTarget("ChildContentRegion", "LightView"));

                    mapper.MapNavigation("Path_TabHeavyView", new NavigationTarget("MainRegion", "TabRegionView"),
                                       new NavigationTarget("TabRegion", "HeavyView"));

                    mapper.MapNavigation("Tab.Tab_A",
                                         new NavigationTarget("MainRegion", "TabRegionView"),
                                         new NavigationTarget("TabRegion", "LightView"))
                                         .WithSegments("Tab", "Tab_A");

                    mapper.MapNavigation("Path_UnknownView", new NavigationTarget("UnknownRegion", "UnknownView"))
                          .WithFallback(new NavigationTarget("MainRegion", "LightView"));
                });

            var sp = services.BuildServiceProvider();
            base.OnStartup(e);
            var converter = sp.GetRequiredService<IconDescriptorConverter>();
            Application.Current.Resources[nameof(IconDescriptorConverter)] = converter;
            var mainWindow = new MainWindow
            {
                DataContext = sp.GetRequiredService<MainWindowViewModel>()
            };
            Current.MainWindow = mainWindow;
            mainWindow.Show();
        }      
    }
}