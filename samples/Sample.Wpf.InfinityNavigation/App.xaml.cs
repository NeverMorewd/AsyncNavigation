using AsyncNavigation.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using ReactiveUI;
using Sample.Common;
using Sample.Wpf.InfinityNavigation.Views;
using Splat;
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

        // Modified by Claude Code:
        // The previous approach (SetRegistrationNamespaces + manually assigning MainThreadScheduler)
        // had a static initialization race: ReactiveCommand`2.ctor reads RxSchedulers.get_MainThreadScheduler()
        // before it accesses RxApp.get_DefaultExceptionHandler() (which is what actually triggers RxApp.cctor).
        // When DataContext assignment fires WPF data bindings and the command is constructed,
        // WPF platform registration has not run yet, so the command permanently captures
        // DefaultScheduler.Instance (thread pool) as its output scheduler, causing cross-thread exceptions.
        // Using InitializeReactiveUI here runs the full WPF platform registration before any View is created,
        // ensuring _mainThreadScheduler is correctly set to WaitForDispatcherScheduler before the first
        // ReactiveCommand is constructed.
        Locator.CurrentMutable.InitializeReactiveUI(RegistrationNamespace.Wpf);

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
