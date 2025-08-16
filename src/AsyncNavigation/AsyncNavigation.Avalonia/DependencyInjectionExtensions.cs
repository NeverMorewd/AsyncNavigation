using AsyncNavigation;
using AsyncNavigation.Abstractions;
using AsyncNavigation.Avalonia;
using AsyncNavigation.Core;
using Avalonia.Controls;
using Avalonia.Controls.Templates;

namespace Microsoft.Extensions.DependencyInjection;

public static class DependencyInjectionExtensions
{
    /// <summary>
    /// Registers the navigation framework services into the dependency injection container.
    /// </summary>
    /// <param name="serviceDescriptors">
    /// The <see cref="IServiceCollection"/> used to register navigation-related services.
    /// </param>
    /// <param name="navigationOptions">
    /// Optional navigation configuration. If provided, it will merge into the global <see cref="NavigationOptions.Default"/>.
    /// </param>
    /// <returns>
    /// The updated <see cref="IServiceCollection"/> with navigation support registered.
    /// </returns>
    /// <remarks>
    /// This method will:
    /// <list type="bullet">
    /// <item>
    /// Merge the specified <paramref name="navigationOptions"/> into <see cref="NavigationOptions.Default"/>.
    /// </item>
    /// <item>
    /// Register <see cref="INavigationTaskManager"/> as a singleton or transient service depending on
    /// <see cref="NavigationOptions.NavigationTaskScope"/>.
    /// </item>
    /// <item>
    /// Register built-in region adapters (<see cref="ContentRegionAdapter"/> and <see cref="ItemsRegionAdapter"/>).
    /// </item>
    /// <item>
    /// Register region navigation, view factory, cache manager, indicator services, and <see cref="IRegionManager"/>.
    /// </item>
    /// </list>
    /// This extension method is typically called during application startup:
    /// <code>
    /// services.AddNavigationSupport(new NavigationOptions
    /// {
    ///     NavigationTaskScope = NavigationTaskScope.Global
    /// });
    /// </code>
    /// </remarks>

    public static IServiceCollection AddNavigationSupport(this IServiceCollection serviceDescriptors, NavigationOptions? navigationOptions = null)
    {
        if (navigationOptions is not null)
        {
            NavigationOptions.Default.MergeFrom(navigationOptions);
        }
        serviceDescriptors.AddSingleton(NavigationOptions.Default);
        if (NavigationOptions.Default.NavigationTaskScope == NavigationTaskScope.App)
        {
            serviceDescriptors.AddSingleton<INavigationTaskManager, NavigationTaskManager>();
        }
        else
        {
            serviceDescriptors.AddTransient<INavigationTaskManager, NavigationTaskManager>();
        }
        return serviceDescriptors
            .RegisterRegionAdapter<ContentRegionAdapter>()
            .RegisterRegionAdapter<ItemsRegionAdapter>()
            .RegisterRegionAdapter<TabRegionAdapter>()
            .AddSingleton<IRegionNavigationServiceFactory, RegionNavigationServiceFactory>()
            .AddSingleton<IRegionFactory, RegionFactory>()
            .AddSingleton<IViewFactory>(sp => new DefaultViewFactory(sp, serviceDescriptors))
            .AddTransient<IViewCacheManager, ViewCacheManager>()
            .AddTransient<IRegionIndicator, RegionIndicator>()
            .AddTransient<IRegionIndicatorManager>(sp => new RegionIndicatorManager(() => sp.GetRequiredService<IRegionIndicator>()))
            .AddSingleton<IRegionManager, RegionManager>();
    }

    /// <summary>
    /// Registers a custom <see cref="IRegionAdapter"/> implementation into the dependency injection container.
    /// </summary>
    /// <typeparam name="T">
    /// The type of the <see cref="IRegionAdapter"/> to register. 
    /// Typically, this adapter is designed to work with a specific container control
    /// (e.g.ContentControl,TabControl).
    /// </typeparam>
    /// <param name="serviceDescriptors">The service collection to add the registration to.</param>
    /// <returns>The same <see cref="IServiceCollection"/> instance for method chaining.</returns>
    public static IServiceCollection RegisterRegionAdapter<T>(this IServiceCollection serviceDescriptors)
        where T : class, IRegionAdapter
    {
        return serviceDescriptors.AddSingleton<IRegionAdapter, T>();
    }

    public static IServiceCollection RegisterLoadingIndicator<T>(this IServiceCollection services) where T : Control =>
        services.RegisterIndicator<T>(NavigationConstants.INDICATOR_LOADING_KEY, o => o.EnsureSingleLoadingIndicator());

    public static IServiceCollection RegisterLoadingIndicator(this IServiceCollection services, Func<IServiceProvider, NavigationContext, Control> builder) =>
        services.RegisterIndicator(NavigationConstants.INDICATOR_LOADING_KEY, o => o.EnsureSingleLoadingIndicator(), builder);

    public static IServiceCollection RegisterErrorIndicator<T>(this IServiceCollection services) where T : Control =>
       services.RegisterIndicator<T>(NavigationConstants.INDICATOR_ERROR_KEY, o => o.EnsureSingleErrorIndicator());

    public static IServiceCollection RegisterErrorIndicator(this IServiceCollection services, Func<IServiceProvider, NavigationContext, Control> builder) =>
        services.RegisterIndicator(NavigationConstants.INDICATOR_ERROR_KEY, o => o.EnsureSingleErrorIndicator(), builder);

    private static IServiceCollection RegisterIndicator<T>(
        this IServiceCollection services,
        string key,
        Action<NavigationOptions> ensureAction) where T : Control
    {
        ensureAction(NavigationOptions.Default);
        return services
            .AddKeyedTransient<T>(key)
            .AddKeyedSingleton<IDataTemplate>(key, (sp, k) =>
                new FuncDataTemplate<NavigationContext>((context, np) =>
                {
                    var indicator = sp.GetRequiredKeyedService<T>(k);
                    indicator.DataContext = context;
                    return indicator;
                }, true));
    }

    private static IServiceCollection RegisterIndicator(
        this IServiceCollection services,
        string key,
        Action<NavigationOptions> ensureAction,
        Func<IServiceProvider, NavigationContext, Control> builder)
    {
        ensureAction(NavigationOptions.Default);
        return services
            .AddKeyedSingleton<IDataTemplate>(key, (sp, k) =>
                new FuncDataTemplate<NavigationContext>((context, np) =>
                {
                    var indicator = builder(sp, context);
                    indicator.DataContext = context;
                    return indicator;
                }, true));
    }
}
