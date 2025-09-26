using AsyncNavigation.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using ReactiveUI;
using Sampe.Wpf.InfinityNaviation.Views;
using Sample.Common;
using System.Configuration;
using System.Data;
using System.Windows;

namespace Sampe.Wpf.InfinityNaviation
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            Current.ShutdownMode = ShutdownMode.OnMainWindowClose;
            PlatformRegistrationManager.SetRegistrationNamespaces(RegistrationNamespace.Wpf);

            var services = new ServiceCollection();
            services.AddNavigationSupport()
                    .RegisterView<InfinityView, InfinityViewModel>(nameof(InfinityView));

            var mainWindow = new MainWindow
            {
                Content = services.BuildServiceProvider().GetRequiredKeyedService<IView>(nameof(InfinityView))
            };
            mainWindow.Show();
        }
    }

}
