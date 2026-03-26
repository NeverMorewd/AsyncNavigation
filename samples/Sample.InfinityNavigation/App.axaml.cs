using AsyncNavigation.Abstractions;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Microsoft.Extensions.DependencyInjection;
using Pipboy.Avalonia;
using Sample.Common;
using Sample.InfinityNavigation.Views;

namespace Sample.InfinityNavigation;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        PipboyThemeManager.Instance.SetPrimaryColor(Color.Parse("#4CAF50"));
        var services = new ServiceCollection();
#pragma warning disable IL2026
        services.AddNavigationSupport()
                .RegisterView<InfinityView, InfinityViewModel>(nameof(InfinityView));
#pragma warning restore IL2026

        var sp = services.BuildServiceProvider();

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow
            {
                Content = sp.GetRequiredKeyedService<IView>(nameof(InfinityView))
            };
        }

        base.OnFrameworkInitializationCompleted();
    }

}