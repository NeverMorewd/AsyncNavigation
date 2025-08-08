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
        AsyncNavigationOptions.EnsureSingleLoadingIndicator();
        return serviceDescriptors
            .AddKeyedTransient<T>(AsyncNavigationConstants.INDICATOR_LOADING_KEY)
            .AddKeyedSingleton<IDataTemplate>(AsyncNavigationConstants.INDICATOR_LOADING_KEY, (sp, key) =>
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
        AsyncNavigationOptions.EnsureSingleLoadingIndicator();
        return serviceDescriptors
            .AddKeyedSingleton<IDataTemplate>(AsyncNavigationConstants.INDICATOR_LOADING_KEY, (sp, key) =>
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
        AsyncNavigationOptions.EnsureSingleErrorIndicator();
        return serviceDescriptors
            .AddKeyedTransient<T>(AsyncNavigationConstants.INDICATOR_ERROR_KEY)
            .AddKeyedSingleton<IDataTemplate>(AsyncNavigationConstants.INDICATOR_ERROR_KEY, (sp, key) =>
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
        AsyncNavigationOptions.EnsureSingleErrorIndicator();
        return serviceDescriptors
            .AddKeyedSingleton<IDataTemplate>(AsyncNavigationConstants.INDICATOR_ERROR_KEY, (sp, key) =>
            {
                return new FuncDataTemplate<NavigationContext>((context, np) =>
                {
                    var loadingIndicator = indicatorBuilder.Invoke(sp, context);
                    loadingIndicator.DataContext = context;
                    return loadingIndicator;
                }, true);
            });
    }
    public static IServiceCollection AddAsyncNavigationSupport(this IServiceCollection serviceDescriptors)
    {
        return serviceDescriptors
            .RegisterRegionAdapter<ContentControlAdapter>()
            .AddTransient(typeof(IRegionNavigationService<>), typeof(RegionNavigationService<>))
            .AddSingleton<RegionFactory>()
            //.AddSingleton<IAsyncViewFactory, AsyncViewFactory>()
            .AddSingleton<IRegionManager, RegionManager>();
    }
}
