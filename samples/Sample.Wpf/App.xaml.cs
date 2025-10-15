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
            Current.ShutdownMode = ShutdownMode.OnExplicitShutdown;
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
                .RegisterInnerIndicatorProvider<InnerIndicatorProvider>()
                .RegisterRegionIndicatorProvider<MessageBoxIndicatorProvider>()
                .RegisterView<ListBoxRegionView, ListBoxRegionViewModel>(nameof(ListBoxRegionView));

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