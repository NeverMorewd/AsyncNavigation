using AsyncNavigation.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using ReactiveUI;
using Sample.Common;
using Sample.Wpf.InfinityNavigation.Views;
using System.Reactive.Concurrency;
using System.Windows;

namespace Sample.Wpf.InfinityNavigation;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{

    protected override void OnStartup(StartupEventArgs e)
    {
        Current.ShutdownMode = ShutdownMode.OnMainWindowClose;
        PlatformRegistrationManager.SetRegistrationNamespaces(RegistrationNamespace.Wpf);

        base.OnStartup(e);

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
