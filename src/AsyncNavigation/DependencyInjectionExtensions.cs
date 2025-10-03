using AsyncNavigation;
using AsyncNavigation.Abstractions;
using AsyncNavigation.Core;

namespace Microsoft.Extensions.DependencyInjection;

public static class DependencyInjectionExtensions
{
    private static readonly Dictionary<Type, Func<IServiceProvider, Type, object>> AwareInterfaceMappings =
        new()
        {
            { typeof(INavigationAware), (sp, vmType) => sp.GetRequiredService(vmType) },
            { typeof(IDialogAware), (sp, vmType) => sp.GetRequiredService(vmType) }
        };
    public static IServiceCollection RegisterView<TView, TViewModel>(
        this IServiceCollection services, object? viewKey = null)
        where TView : class, IView
        where TViewModel : class
    {
        ArgumentNullException.ThrowIfNull(services);
        viewKey ??= typeof(TView);

        services.AddTransient<TViewModel>();
        services.AddTransient<TView>();

        services.AddKeyedTransient<IView>(viewKey, (sp, _) =>
        {
            var view = sp.GetRequiredService<TView>();
            view.DataContext ??= sp.GetRequiredService<TViewModel>();
            return view;
        });

        var vmType = typeof(TViewModel);
        bool hasMapping = false;

        foreach (var awareFace in AwareInterfaceMappings)
        {
            if (awareFace.Key.IsAssignableFrom(vmType))
            {
                hasMapping = true;
                services.AddKeyedTransient(awareFace.Key, viewKey, (sp, _) => awareFace.Value(sp, vmType));
            }
        }
        if (!hasMapping)
        {
            throw new InvalidOperationException($"ViewModel type '{vmType.Name}' must implement at least one of the following interfaces: {string.Join(", ", AwareInterfaceMappings.Keys.Select(t => t.Name))}");
        }
        return services;
    }
    public static IServiceCollection RegisterRegionIndicatorProvider<T>(this IServiceCollection services) where T : class, IRegionIndicatorProvider
    {
        ArgumentNullException.ThrowIfNull(services);
        return services.AddSingleton<IRegionIndicatorProvider, T>();
    }
    internal static IServiceCollection RegisterNavigationFramework(this IServiceCollection serviceDescriptors, NavigationOptions? navigationOptions = null)
    {
        if (navigationOptions is not null)
        {
            NavigationOptions.Default.MergeFrom(navigationOptions);
        }


#if GC_TEST
        NavigationOptions.Default.MaxHistoryItems = 0;
#endif

        serviceDescriptors.AddSingleton(NavigationOptions.Default);
        if (NavigationOptions.Default.NavigationJobScope == NavigationJobScope.App)
        {
            serviceDescriptors.AddSingleton<IJobScheduler, JobScheduler>();
            //serviceDescriptors.AddSingleton<JobScheduler>();
        }
        else
        {
            serviceDescriptors.AddTransient<IJobScheduler, JobScheduler>();
        }
        return serviceDescriptors
            .AddSingleton<IDialogService, DialogService>()
            .AddSingleton<IRegionNavigationServiceFactory, RegionNavigationServiceFactory>()
            .AddSingleton<IRegionFactory, RegionFactory>()
            .AddSingleton<IViewFactory, DefaultViewFactory>()
            .AddTransient<IViewManager, ViewManager>()
            .AddTransient<IRegionNavigationHistory, RegionNavigationHistory>()
            .AddTransient<IRegionIndicatorProvider, RegionIndicatorProvider>()
            .AddTransient<IRegionIndicatorManager, RegionIndicatorManager>();
    }
}
