using AsyncNavigation.Core;
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
                .RegisterView<AView, AViewModel>(nameof(AView))
                .RegisterView<BView, BViewModel>(nameof(BView))
                .RegisterView<CView, CViewModel>(nameof(CView))
                .RegisterView<DView, DViewModel>(nameof(DView))
                .RegisterView<EView, EViewModel>(nameof(EView))
                .RegisterDialogWindow<AWindow, AViewModel>(nameof(AWindow))
                .RegisterInnerIndicatorProvider<InnerIndicatorProvider>()
                .RegisterRegionIndicatorProvider<MessageBoxIndicatorProvider>()
                .RegisterView<ListBoxRegionView, ListBoxRegionViewModel>(nameof(ListBoxRegionView))
                .RegisterRouter((mapper, sp) =>
                {
                    mapper.MapNavigation("Path_ChildEView",
                                         new NavigationTarget("MainRegion", "CView"),
                                         new NavigationTarget("ChildContentRegion", "EView"));

                    mapper.MapNavigation("Path_ChildAView",
                                         new NavigationTarget("MainRegion", "CView"),
                                         new NavigationTarget("ChildContentRegion", "AView"));

                    mapper.MapNavigation("Path_TabEView", new NavigationTarget("MainRegion", "DView"),
                                       new NavigationTarget("TabRegion", "EView"));

                    mapper.MapNavigation("Tab.Tab_A",
                                         new NavigationTarget("MainRegion", "DView"),
                                         new NavigationTarget("TabRegion", "AView"))
                                         .WithSegments("Tab", "Tab_A");

                    mapper.MapNavigation("Path_UnknownView", new NavigationTarget("UnknownRegion", "UnknownView"))
                          .WithFallback(new NavigationTarget("MainRegion", "AView"));
                });

            var sp = services.BuildServiceProvider();
            base.OnStartup(e);

            var mainWindow = new MainWindow
            {
                DataContext = sp.GetRequiredService<MainWindowViewModel>()
            };
            Current.MainWindow = mainWindow;
            mainWindow.Show();
        }      
    }
}