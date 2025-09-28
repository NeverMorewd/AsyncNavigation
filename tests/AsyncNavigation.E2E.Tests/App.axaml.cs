using AsyncNavigation.Core;
using Avalonia;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;

namespace AsyncNavigation.E2E.Tests;


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
            NavigationJobStrategy = NavigationJobStrategy.CancelCurrent
        };

        var services = new ServiceCollection();
        services.AddNavigationSupport(navigationOptions);

        base.OnFrameworkInitializationCompleted();
    }
}
