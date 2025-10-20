using AsyncNavigation;
using AsyncNavigation.Abstractions;
using AsyncNavigation.Core;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.Extensions.DependencyInjection;

public static class DependencyInjectionExtensions
{
    private static readonly Dictionary<Type, Func<IServiceProvider, object, Type, object>> AwareInterfaceMappings =
        new()
        {
            { typeof(INavigationAware), (sp, key, vmType) => sp.GetRequiredKeyedService(vmType,key) },
            { typeof(IDialogAware), (sp, key, vmType) => sp.GetRequiredKeyedService(vmType,key) }
        };

    [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(RegionContext))]
    internal static IServiceCollection RegisterNavigationFramework(this IServiceCollection serviceDescriptors, NavigationOptions? navigationOptions = null)
    {
        if (navigationOptions is not null)
        {
            NavigationOptions.Default.MergeFrom(navigationOptions);
        }


#if GC_TEST
        NavigationOptions.Default.MaxHistoryItems = 2;
#endif

        serviceDescriptors.AddSingleton(NavigationOptions.Default);
        if (NavigationOptions.Default.NavigationJobScope == NavigationJobScope.App)
        {
            serviceDescriptors.AddSingleton<IJobScheduler, JobScheduler>();
        }
        else
        {
            serviceDescriptors.AddTransient<IJobScheduler, JobScheduler>();
        }
        return serviceDescriptors
            .AddSingleton<IDialogService, DialogService>()
            .AddSingleton<IRegionNavigationServiceFactory, RegionNavigationServiceFactory>()
            .AddSingleton<IRegionFactory, RegionFactory>()
            .AddSingleton<IViewFactory, ViewFactory>()
            .AddTransient<IViewManager, ViewManager>()
            .AddTransient<IRegionNavigationHistory, RegionNavigationHistory>()
            .AddTransient<IRegionIndicatorProvider, RegionIndicatorProvider>()
            .AddTransient<IRegionIndicatorManager, RegionIndicatorManager>();
    }

    /// <summary>
    /// Registers a view and its corresponding view model with the specified view key.
    /// The view must implement IView interface and both view and view model will be registered as transient services.
    /// Additionally, registers the view as a keyed IView service and ensures the view model implements at least one required interface.
    /// </summary>
    /// <typeparam name="TView">The type of the view to register, must implement IView interface</typeparam>
    /// <typeparam name="TViewModel">The type of the view model to register</typeparam>
    /// <param name="services">The IServiceCollection to add the services to</param>
    /// <param name="viewKey">The key to use for registering the view as a keyed service</param>
    /// <returns>The IServiceCollection so that additional calls can be chained</returns>
    /// <exception cref="InvalidOperationException">Thrown when the view model doesn't implement any of the required interfaces</exception>
    public static IServiceCollection RegisterView<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TView,
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TViewModel>(this IServiceCollection services, object viewKey)
            where TView : class, IView
            where TViewModel : class
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(viewKey);

        services.AddKeyedTransient<TViewModel>(viewKey);
        services.AddKeyedTransient<TView>(viewKey);

        services.AddKeyedTransient<IView>(viewKey, (sp, key) =>
        {
            var view = sp.GetRequiredKeyedService<TView>(key);
            view.DataContext ??= sp.GetRequiredKeyedService<TViewModel>(key);
            return view;
        });

        var vmType = typeof(TViewModel);
        bool hasMapping = false;

        foreach (var awareFace in AwareInterfaceMappings)
        {
            if (awareFace.Key.IsAssignableFrom(vmType))
            {
                hasMapping = true;
                services.AddKeyedTransient(awareFace.Key, viewKey, (sp, key) => awareFace.Value(sp, key!, vmType));
            }
        }
        if (!hasMapping)
        {
            throw new InvalidOperationException($"ViewModel type '{vmType.Name}' must implement at least one of the following interfaces: {string.Join(", ", AwareInterfaceMappings.Keys.Select(t => t.Name))}");
        }
        return services;
    }
    public static IServiceCollection RegisterNavigation<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TView,
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TViewModel>(this IServiceCollection services, object viewKey)
            where TView : class, IView
            where TViewModel : class, INavigationAware
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(viewKey);

        services.AddKeyedTransient<TViewModel>(viewKey);
        services.AddKeyedTransient<TView>(viewKey);

        services.AddKeyedTransient<IView>(viewKey, (sp, key) =>
        {
            var view = sp.GetRequiredKeyedService<TView>(key);
            view.DataContext ??= sp.GetRequiredKeyedService<TViewModel>(key);
            return view;
        });
        return services;
    }
    public static IServiceCollection RegisterNavigation<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TView,
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TViewModel>(this IServiceCollection services,
        object viewKey,
        Func<IServiceProvider, object?, TViewModel> viewModelBuilder)
        where TView : class, IView
        where TViewModel : class, INavigationAware
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(viewKey);
        ArgumentNullException.ThrowIfNull(viewModelBuilder);

        services.AddKeyedTransient<TView>(viewKey);
        services.AddKeyedTransient(viewKey, viewModelBuilder);
        services.AddKeyedTransient<IView>(viewKey, (sp, key) =>
        {
            var view = sp.GetRequiredKeyedService<TView>(key);
            view.DataContext ??= sp.GetRequiredKeyedService<TViewModel>(key);
            return view;
        });
        return services;
    }
    public static IServiceCollection RegisterDialog<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TView,
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TViewModel>(this IServiceCollection services, object viewKey)
            where TView : class, IView
            where TViewModel : class, IDialogAware
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(viewKey);

        services.AddKeyedTransient<TViewModel>(viewKey);
        services.AddKeyedTransient<TView>(viewKey);

        services.AddKeyedTransient<IView>(viewKey, (sp, key) =>
        {
            var view = sp.GetRequiredKeyedService<TView>(key);
            view.DataContext ??= sp.GetRequiredKeyedService<TViewModel>(key);
            return view;
        });
        return services;
    }
    public static IServiceCollection RegisterDialog<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TView,
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TViewModel>(this IServiceCollection services,
        object viewKey,
        Func<IServiceProvider, object?, TViewModel> viewModelBuilder)
        where TView : class, IView
        where TViewModel : class, IDialogAware
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(viewKey);
        ArgumentNullException.ThrowIfNull(viewModelBuilder);

        services.AddKeyedTransient<TView>(viewKey);
        services.AddKeyedTransient(viewKey, viewModelBuilder);
        services.AddKeyedTransient<IView>(viewKey, (sp, key) =>
        {
            var view = sp.GetRequiredKeyedService<TView>(viewKey);
            view.DataContext ??= sp.GetRequiredKeyedService<TViewModel>(key);
            return view;
        });
        return services;
    }
    /// <summary>
    /// Registers a region indicator provider as a singleton service.
    /// The provider must implement IRegionIndicatorProvider interface.
    /// </summary>
    /// <typeparam name="T">The type of the region indicator provider to register, must implement IRegionIndicatorProvider</typeparam>
    /// <param name="services">The IServiceCollection to add the service to</param>
    /// <returns>The IServiceCollection so that additional calls can be chained</returns>
    public static IServiceCollection RegisterRegionIndicatorProvider<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] T>(this IServiceCollection services) where T : class, IRegionIndicatorProvider
    {
        ArgumentNullException.ThrowIfNull(services);
        return services.AddSingleton<IRegionIndicatorProvider, T>();
    }

    /// <summary>
    /// Registers a dialog window as a keyed transient service.
    /// The window must implement IDialogWindow interface.
    /// If no window name is provided, the type name will be used as the key.
    /// </summary>
    /// <typeparam name="T">The type of the dialog window to register, must implement IDialogWindow</typeparam>
    /// <param name="serviceDescriptors">The IServiceCollection to add the service to</param>
    /// <param name="windowName">The optional name to use as the key for the dialog window registration</param>
    /// <returns>The IServiceCollection so that additional calls can be chained</returns>
    public static IServiceCollection RegisterDialogWindow<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] T>(this IServiceCollection serviceDescriptors, string? windowName = null)
      where T : class, IDialogWindow
    {
        windowName ??= typeof(T).Name;
        serviceDescriptors.AddKeyedTransient<IDialogWindow, T>(windowName);
        return serviceDescriptors;
    }

    /// <summary>
    /// Registers a service as a singleton with all members dynamically accessible.
    /// This method preserves dynamic access to all members of the type during trimming.
    /// </summary>
    /// <typeparam name="T">The type of the service to register</typeparam>
    /// <param name="serviceDescriptors">The IServiceCollection to add the service to</param>
    /// <returns>The IServiceCollection so that additional calls can be chained</returns>
    public static IServiceCollection AddSingletonWitAllMembers<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] T>(this IServiceCollection serviceDescriptors) where T : class
    {
        return serviceDescriptors.AddSingleton<T>();
    }

    /// <summary>
    /// Registers a service as transient with all members dynamically accessible.
    /// This method preserves dynamic access to all members of the type during trimming.
    /// </summary>
    /// <typeparam name="T">The type of the service to register</typeparam>
    /// <param name="serviceDescriptors">The IServiceCollection to add the service to</param>
    /// <returns>The IServiceCollection so that additional calls can be chained</returns>
    public static IServiceCollection AddTransientWitAllMembers<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] T>(this IServiceCollection serviceDescriptors) where T : class
    {
        return serviceDescriptors.AddTransient<T>();
    }
}
