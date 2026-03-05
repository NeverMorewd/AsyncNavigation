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
        // SetRegistrationNamespaces restricts platform registration to WPF only, but it does not trigger
        // RxApp.cctor by itself. RxApp.cctor is what fires the Splat resolver callback that actually runs
        // Wpf.Registrations.Register() and sets RxSchedulers._mainThreadScheduler = WaitForDispatcherScheduler.
        //
        // The bug: ReactiveCommand`2.ctor calls RxSchedulers.get_MainThreadScheduler() *before* calling
        // RxApp.get_DefaultExceptionHandler() (the latter is what triggers RxApp.cctor). So when
        // DataContext assignment fires WPF data bindings and the first ReactiveCommand is constructed,
        // _mainThreadScheduler is still null and gets permanently initialized to DefaultScheduler.Instance
        // (thread pool), causing cross-thread exceptions at command execution time.
        //
        // Fix: reading RxApp.MainThreadScheduler here forces RxApp.cctor to run immediately, which fires
        // the Splat callback with NamespacesToRegister=[Wpf] and sets the correct WaitForDispatcherScheduler,
        // before any View DataContext is assigned and any ReactiveCommand is constructed.
        PlatformRegistrationManager.SetRegistrationNamespaces(RegistrationNamespace.Wpf);
        Locator.CurrentMutable.InitializeReactiveUI(RegistrationNamespace.Wpf);
        //_ = RxApp.MainThreadScheduler;

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
