using AsyncNavigation;
using AsyncNavigation.Core;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
using Sample.Avalonia.Regions;
using Sample.Avalonia.Views;
using Sample.Common;
using System.Runtime.InteropServices;

namespace Sample.Avalonia;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        NavigationOptions navigationOptions = new()
        {
            /// default is CancelCurrent <see cref="NavigationJobStrategy.CancelCurrent"/>
            NavigationJobStrategy = NavigationJobStrategy.CancelCurrent
        };

        var services = new ServiceCollection();
        services.AddNavigationSupport(navigationOptions)
                .AddSingleton<MainWindowViewModel>()
                .RegisterView<AView, AViewModel>(nameof(AView))
                .RegisterView<BView, BViewModel>(nameof(BView))
                .RegisterView<CView, CViewModel>(nameof(CView))
                .RegisterView<DView, DViewModel>(nameof(DView))
                .RegisterView<EView, EViewModel>(nameof(EView))
                .RegisterView<ListBoxRegionView, ListBoxRegionViewModel>(nameof(ListBoxRegionView))
                .RegisterRegionIndicatorProvider<NotifyIndicatorProvider>()
                .RegisterInnerIndicatorProvider<InnerIndicatorProvider>()
                .RegisterRegionAdapter<ListBoxRegionAdapter>();

        #region setup lifetime
        var sp = services.BuildServiceProvider();
        var viewModel = sp.GetRequiredService<MainWindowViewModel>();

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow
            {
                DataContext = viewModel
            };
        }
        else if (ApplicationLifetime is ISingleViewApplicationLifetime singleViewPlatform)
        {
            singleViewPlatform.MainView = new MainView
            {
                DataContext = viewModel
            };
        }
        #endregion

        base.OnFrameworkInitializationCompleted();
    }
}