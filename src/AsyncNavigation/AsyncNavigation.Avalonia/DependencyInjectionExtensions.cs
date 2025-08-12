using AsyncNavigation;
using AsyncNavigation.Abstractions;
using AsyncNavigation.Avalonia;
using AsyncNavigation.Core;
using Avalonia.Controls;
using Avalonia.Controls.Templates;

namespace Microsoft.Extensions.DependencyInjection;

public static class DependencyInjectionExtensions
{
    public static IServiceCollection RegisterRegionAdapter<T>(this IServiceCollection serviceDescriptors)
        where T : class, IRegionAdapter
    {
        return serviceDescriptors.AddSingleton<IRegionAdapter, T>();
    }
    public static IServiceCollection RegisterLoadingIndicator<T>(this IServiceCollection serviceDescriptors)
        where T : Control
    {
        NavigationOptions.Default.EnsureSingleLoadingIndicator();
        return serviceDescriptors
            .AddKeyedTransient<T>(NavigationConstants.INDICATOR_LOADING_KEY)
            .AddKeyedSingleton<IDataTemplate>(NavigationConstants.INDICATOR_LOADING_KEY, (sp, key) =>
            {
                return new FuncDataTemplate<NavigationContext>((context, np) =>
                {
                    var loadingIndicator = sp.GetRequiredKeyedService<T>(key);
                    loadingIndicator.DataContext = context;
                    return loadingIndicator;
                }, true);
            });
    }
    public static IServiceCollection RegisterLoadingIndicator(this IServiceCollection serviceDescriptors, Func<IServiceProvider, NavigationContext, Control> indicatorBuilder)
    {
        NavigationOptions.Default.EnsureSingleLoadingIndicator();
        return serviceDescriptors
            .AddKeyedSingleton<IDataTemplate>(NavigationConstants.INDICATOR_LOADING_KEY, (sp, key) =>
            {
                return new FuncDataTemplate<NavigationContext>((context, np) =>
                {
                    var loadingIndicator = indicatorBuilder.Invoke(sp, context);
                    return loadingIndicator;
                }, true);
            });
    }
    public static IServiceCollection RegisterErrorIndicator<T>(this IServiceCollection serviceDescriptors)
        where T : Control
    {
        NavigationOptions.Default.EnsureSingleErrorIndicator();
        return serviceDescriptors
            .AddKeyedTransient<T>(NavigationConstants.INDICATOR_ERROR_KEY)
            .AddKeyedSingleton<IDataTemplate>(NavigationConstants.INDICATOR_ERROR_KEY, (sp, key) =>
            {
                return new FuncDataTemplate<NavigationContext>((context, np) =>
                {
                    var loadingIndicator = sp.GetRequiredKeyedService<T>(key);
                    loadingIndicator.DataContext = context;
                    return loadingIndicator;
                }, true);
            });
    }
    public static IServiceCollection RegisterErrorIndicator(this IServiceCollection serviceDescriptors, Func<IServiceProvider, NavigationContext, Control> indicatorBuilder)
    {
        NavigationOptions.Default.EnsureSingleErrorIndicator();
        return serviceDescriptors
            .AddKeyedSingleton<IDataTemplate>(NavigationConstants.INDICATOR_ERROR_KEY, (sp, key) =>
            {
                return new FuncDataTemplate<NavigationContext>((context, np) =>
                {
                    var loadingIndicator = indicatorBuilder.Invoke(sp, context);
                    loadingIndicator.DataContext = context;
                    return loadingIndicator;
                }, true);
            });
    }
    public static IServiceCollection AddAsyncNavigationSupport(this IServiceCollection serviceDescriptors, NavigationOptions? navigationOptions = null)
    {
        if(navigationOptions is not null)
        {
            NavigationOptions.Default.MergeFrom(navigationOptions);
        }
        if (NavigationOptions.Default.NavigationTaskScope == NavigationTaskScope.Global)
        {
            serviceDescriptors.AddSingleton<INavigationTaskManager, NavigationTaskManager>();
        }
        else
        {
            serviceDescriptors.AddTransient<INavigationTaskManager, NavigationTaskManager>();
        }
        return serviceDescriptors
            .RegisterRegionAdapter<ContentControlAdapter>()
            .RegisterRegionAdapter<ItemsControlAdapter>()
            .AddTransient(typeof(IRegionNavigationService<>), typeof(RegionNavigationService<>))
            .AddSingleton<RegionFactory>()
            .AddSingleton<IViewFactory<Control>>(sp => new DefaultViewFactory<Control>(sp, serviceDescriptors))
            .AddTransient<IViewCacheManager, ViewCacheManager>()
            .AddTransient<IRegionIndicatorManager<ContentControl>, RegionIndicatorManager>()
            .AddSingleton<IRegionManager, RegionManager>();
    }
}
