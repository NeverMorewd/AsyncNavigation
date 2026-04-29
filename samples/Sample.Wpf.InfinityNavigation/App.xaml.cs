using AsyncNavigation.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using ReactiveUI.Builder;
using Sample.Common;
using Sample.Wpf.InfinityNavigation.Views;
using System.Windows;

namespace Sample.Wpf.InfinityNavigation;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{

    protected override void OnStartup(StartupEventArgs e)
    {
        RxAppBuilder.CreateReactiveUIBuilder()
                    .WithWpf()
                    .BuildApp();
        Current.ShutdownMode = ShutdownMode.OnMainWindowClose;

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
