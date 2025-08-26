using AsyncNavigation;
using AsyncNavigation.Abstractions;
using AsyncNavigation.Core;

namespace Microsoft.Extensions.DependencyInjection;

public static class DependencyInjectionExtensions
{
    public static IServiceCollection RegisterNavigation<TView, TViewModel>(
        this IServiceCollection services, string viewKey)
        where TView : class, IView
        where TViewModel : class, INavigationAware
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentException.ThrowIfNullOrWhiteSpace(viewKey);

        services.AddTransient<TViewModel>();
        services.AddTransient<TView>();

        services.AddKeyedTransient<IView>(viewKey, (sp, _) => sp.GetRequiredService<TView>());
        services.AddKeyedTransient<INavigationAware>(viewKey, (sp, _) => sp.GetRequiredService<TViewModel>());

        if (typeof(IDialogAware).IsAssignableFrom(typeof(TViewModel)))
        {
            services.AddKeyedTransient(viewKey, (sp, _) =>
                (IDialogAware)sp.GetRequiredService<TViewModel>());
        }

        return services;
    }

    internal static IServiceCollection RegisterNavigationFramework(this IServiceCollection serviceDescriptors, NavigationOptions? navigationOptions = null)
    {
        if (navigationOptions is not null)
        {
            NavigationOptions.Default.MergeFrom(navigationOptions);
        }
        serviceDescriptors.AddSingleton(NavigationOptions.Default);
        if (NavigationOptions.Default.NavigationJobScope == NavigationJobScope.App)
        {
            serviceDescriptors.AddSingleton<INavigationJobScheduler, NavigationJobScheduler>();
        }
        else
        {
            serviceDescriptors.AddTransient<INavigationJobScheduler, NavigationJobScheduler>();
        }
        return serviceDescriptors
            .AddSingleton<IDialogService, DialogService>()
            .AddSingleton<IRegionNavigationServiceFactory, RegionNavigationServiceFactory>()
            .AddSingleton<IRegionFactory, RegionFactory>()
            .AddSingleton<IViewFactory>(sp => new DefaultViewFactory(sp, serviceDescriptors))
            .AddTransient<IViewManager, ViewManager>()
            .AddTransient<IRegionNavigationHistory, RegionNavigationHistory>()
            .AddTransient<IRegionIndicatorManager>(sp => new RegionIndicatorManager(() => sp.GetRequiredService<IRegionIndicator>()));
    }

}
