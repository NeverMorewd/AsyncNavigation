using Microsoft.Extensions.DependencyInjection;
using ReactiveUI;
using Sample.Common;
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
            ReactiveUI.PlatformRegistrationManager.SetRegistrationNamespaces(RegistrationNamespace.Wpf);
            var services = new ServiceCollection();
            services.AddNavigationSupport()
                .AddSingleton<MainWindowViewModel>()
                    .RegisterView<AView, AViewModel>(nameof(AView));

            var sp = services.BuildServiceProvider();
            base.OnStartup(e);

            var mainWindow = new MainWindow();
            mainWindow.DataContext = sp.GetRequiredService<MainWindowViewModel>();
            Current.MainWindow = mainWindow;
            mainWindow.Show();
        }
    }
}