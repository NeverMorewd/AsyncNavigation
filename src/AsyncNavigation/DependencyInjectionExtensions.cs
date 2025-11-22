using AsyncNavigation;
using AsyncNavigation.Abstractions;
using AsyncNavigation.Core;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.Extensions.DependencyInjection;

public static class DependencyInjectionExtensions
{
    private static readonly Dictionary<Type, Func<IServiceProvider, Type, object>> AwareInterfaceMappings =
        new()
        {
            { typeof(INavigationAware), (sp, vmType) => sp.GetRequiredService(vmType) },
            { typeof(IDialogAware), (sp, vmType) => sp.GetRequiredService(vmType) }
        };

    /// <summary>
    /// Registers core services required by the AsyncNavigation framework.
    /// Optionally merges provided navigation options with the default settings.
    /// </summary>
    [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(RegionContext))]
    internal static IServiceCollection RegisterNavigationFramework(this IServiceCollection serviceDescriptors, NavigationOptions? navigationOptions = null)
    {
        if (navigationOptions is not null)
        {
            NavigationOptions.Default.MergeFrom(navigationOptions);
        }

        serviceDescriptors.AddSingleton(NavigationOptions.Default);
        if (NavigationOptions.Default.NavigationJobScope == NavigationJobScope.App)
        {
            serviceDescriptors.AddSingleton<IAsyncJobProcessor, AsyncJobProcessor>();
        }
        else
        {
            serviceDescriptors.AddTransient<IAsyncJobProcessor, AsyncJobProcessor>();
        }
        return serviceDescriptors
            .AddSingleton<IDialogService, DialogService>()
            .AddSingleton<IRegionNavigationServiceFactory, RegionNavigationServiceFactory>()
            .AddSingleton<IRegionFactory, RegionFactory>()
            .AddSingleton<IViewFactory, ViewFactory>()
            .AddSingleton<IRegistrationTracker>(RegistrationTracker.Instance)
            .AddTransient<IViewManager, ViewManager>()
            .AddTransient<IRegionNavigationHistory, RegionNavigationHistory>()
            .AddTransient<IRegionIndicatorProvider, RegionIndicatorProvider>()
            .AddTransient<IRegionIndicatorManager, RegionIndicatorManager>();
    }

    private static IServiceCollection RegisterViewModelAndView<TView, TViewModel, TAware>(
        this IServiceCollection services,
        string name,
        Func<IServiceProvider, TViewModel>? viewModelBuilder = null)
        where TView : class, IView
        where TViewModel : class, TAware
        where TAware : class
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(name);

        services.AddTransient<TView>();
        if (viewModelBuilder is null)
            services.AddTransient<TViewModel>();
        else
            services.AddTransient(sp => viewModelBuilder(sp));

        services.AddKeyedTransient<TAware>(name, (sp, _) =>
        {
            return sp.GetRequiredService<TViewModel>();
        });
        services.AddKeyedTransient<IView>(name, (sp, key) =>
        {
            var view = sp.GetRequiredService<TView>();
            view.DataContext = sp.GetRequiredKeyedService<TAware>(key);
            return view;
        });
        return services;
    }

    private static IServiceCollection RegisterDialogWindowInternal<TWindow, TViewModel>(
        this IServiceCollection services,
        string windowName,
        Func<IServiceProvider, TViewModel>? viewModelBuilder = null)
        where TWindow : class, IDialogWindow
        where TViewModel : class, IDialogAware
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(windowName);

        services.AddTransient<TWindow>();
        if (viewModelBuilder is null)
            services.AddTransient<TViewModel>();
        else
            services.AddTransient(sp => viewModelBuilder(sp));
        services.AddKeyedTransient<IDialogAware>(windowName, (sp, _) =>
        {
            return sp.GetRequiredService<TViewModel>();
        });
        services.AddKeyedTransient<IDialogWindow>(windowName, (sp, key) =>
        {
            var window = sp.GetRequiredService<TWindow>();
            window.DataContext = sp.GetRequiredKeyedService<IDialogAware>(key);
            return window;
        });
        return services;
    }

    /// <summary>
    /// Registers a view and its corresponding view model with the specified view key.
    /// </summary>
    public static IServiceCollection RegisterView<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TView,
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TViewModel>(this IServiceCollection services, string name)
            where TView : class, IView
            where TViewModel : class
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(name);

        services.AddTransient<TViewModel>();
        services.AddTransient<TView>();

        services.AddKeyedTransient<IView>(name, (sp, _) =>
        {
            var view = sp.GetRequiredService<TView>();
            view.DataContext = sp.GetRequiredService<TViewModel>();
            return view;
        });

        var vmType = typeof(TViewModel);
        var hasMapping = false;
        foreach (var awareFace in AwareInterfaceMappings.Where(awareFace => awareFace.Key.IsAssignableFrom(vmType)))
        {
            hasMapping = true;
            services.AddKeyedTransient(awareFace.Key, name, (sp, _) => awareFace.Value(sp, vmType));
        }
        if (!hasMapping)
        {
            throw new InvalidOperationException($"ViewModel type '{vmType.Name}' must implement at least one of the following interfaces: {string.Join(", ", AwareInterfaceMappings.Keys.Select(t => t.Name))}");
        }
        RegistrationTracker.Instance.TrackView(name);
        return services;
    }

    /// <summary>
    /// Register for Navigation
    /// </summary>
    /// <typeparam name="TView"></typeparam>
    /// <typeparam name="TViewModel"></typeparam>
    /// <param name="services"></param>
    /// <param name="name"></param>
    /// <param name="viewModelBuilder"></param>
    /// <returns></returns>
    public static IServiceCollection RegisterNavigation<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TView,
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TViewModel>(
        this IServiceCollection services,
        string name,
        Func<IServiceProvider, TViewModel>? viewModelBuilder = null)
        where TView : class, IView
        where TViewModel : class, INavigationAware
    {
        RegistrationTracker.Instance.TrackNavigation(name);
        return services.RegisterViewModelAndView<TView, TViewModel, INavigationAware>(name, viewModelBuilder);
    }
    /// <summary>
    /// Register for Navigation
    /// </summary>
    /// <typeparam name="TView"></typeparam>
    /// <typeparam name="TViewModel"></typeparam>
    /// <param name="services"></param>
    /// <param name="name"></param>
    /// <returns></returns>
    public static IServiceCollection RegisterNavigation<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TView,
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TViewModel>(this IServiceCollection services, string name)
            where TView : class, IView
            where TViewModel : class, INavigationAware
        => RegisterNavigation<TView, TViewModel>(services, name, null);


    /// <summary>
    /// Register for Dialog
    /// </summary>
    /// <typeparam name="TView"></typeparam>
    /// <typeparam name="TViewModel"></typeparam>
    /// <param name="services"></param>
    /// <param name="name"></param>
    /// <param name="viewModelBuilder"></param>
    /// <returns></returns>
    public static IServiceCollection RegisterDialog<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TView,
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TViewModel>(
        this IServiceCollection services,
        string name,
        Func<IServiceProvider, TViewModel>? viewModelBuilder = null)
        where TView : class, IView
        where TViewModel : class, IDialogAware
    {
        RegistrationTracker.Instance.TrackDialog(name);
        return services.RegisterViewModelAndView<TView, TViewModel, IDialogAware>(name, viewModelBuilder);
    }
    /// <summary>
    /// Register for Dialog
    /// </summary>
    /// <typeparam name="TView"></typeparam>
    /// <typeparam name="TViewModel"></typeparam>
    /// <param name="services"></param>
    /// <param name="name"></param>
    /// <returns></returns>
    public static IServiceCollection RegisterDialog<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TView,
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TViewModel>(this IServiceCollection services, string name)
            where TView : class, IView
            where TViewModel : class, IDialogAware
        => RegisterDialog<TView, TViewModel>(services, name, null);

    /// <summary>
    /// Registers a region indicator provider as a singleton service.
    /// </summary>
    public static IServiceCollection RegisterRegionIndicatorProvider<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] T>(this IServiceCollection services)
        where T : class, IRegionIndicatorProvider
    {
        ArgumentNullException.ThrowIfNull(services);
        return services.AddSingleton<IRegionIndicatorProvider, T>();
    }

    /// <summary>
    /// Registers a window as a dialog container.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="serviceDescriptors"></param>
    /// <param name="windowName"></param>
    /// <returns></returns>
    public static IServiceCollection RegisterDialogContainer<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] T>(this IServiceCollection serviceDescriptors, string windowName)
      where T : class, IDialogWindow
    {
        if (windowName == NavigationConstants.DEFAULT_DIALOG_WINDOW_KEY)
        {
            serviceDescriptors.TryAddKeyedTransient<IDialogWindow, T>(windowName);
        }
        else
        {
            serviceDescriptors.AddKeyedTransient<IDialogWindow, T>(windowName);
        }
        return serviceDescriptors;
    }
    
    /// <summary>
    /// Override Default DialogContainer
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="serviceDescriptors"></param>
    /// <returns></returns>
    public static IServiceCollection OverrideDefaultDialogContainer<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] T>(this IServiceCollection serviceDescriptors)
      where T : class, IDialogWindow
    {
        serviceDescriptors.AddKeyedTransient<IDialogWindow, T>(NavigationConstants.DEFAULT_DIALOG_WINDOW_KEY);
        return serviceDescriptors;
    }

    /// <summary>
    /// Registers a service as a singleton with all members dynamically accessible.
    /// </summary>
    public static IServiceCollection AddSingletonWitAllMembers<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] T>(
        this IServiceCollection serviceDescriptors) where T : class
    {
        return serviceDescriptors.AddSingleton<T>();
    }
    /// <summary>
    /// Registers a service as a singleton with all members dynamically accessible.
    /// </summary>
    public static IServiceCollection AddSingletonWitAllMembers<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] T>(
        this IServiceCollection serviceDescriptors, Func<IServiceProvider, T> builder) where T : class
    {
        return serviceDescriptors.AddSingleton(builder);
    }
    /// <summary>
    /// Registers a service as a transient with all members dynamically accessible.
    /// </summary>
    public static IServiceCollection AddTransientWitAllMembers<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] T>(
        this IServiceCollection serviceDescriptors) where T : class
    {
        return serviceDescriptors.AddTransient<T>();
    }
    /// <summary>
    /// Registers a service as a transient with all members dynamically accessible.
    /// </summary>
    public static IServiceCollection AddTransientWitAllMembers<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] T>(
        this IServiceCollection serviceDescriptors, Func<IServiceProvider, T> builder) where T : class
    {
        return serviceDescriptors.AddTransient(builder);
    }
    /// <summary>
    /// Registers a dialog window and its associated view model using the specified window name as a key.
    /// </summary>
    /// <typeparam name="TWindow"></typeparam>
    /// <typeparam name="TViewModel"></typeparam>
    /// <param name="services"></param>
    /// <param name="windowName"></param>
    /// <param name="viewModelBuilder"></param>
    /// <returns></returns>
    public static IServiceCollection RegisterDialogWindow<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TWindow,
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TViewModel>(
        this IServiceCollection services,
        string windowName,
        Func<IServiceProvider, TViewModel>? viewModelBuilder = null)
        where TWindow : class, IDialogWindow
        where TViewModel : class, IDialogAware
    {
        return services.RegisterDialogWindowInternal<TWindow, TViewModel>(windowName, viewModelBuilder);
    }
    /// <summary>
    /// Registers a dialog window and its associated view model using the specified window name as a key.
    /// </summary>
    /// <typeparam name="TWindow"></typeparam>
    /// <typeparam name="TViewModel"></typeparam>
    /// <param name="services"></param>
    /// <param name="windowName"></param>
    /// <returns></returns>
    public static IServiceCollection RegisterDialogWindow<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TWindow,
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TViewModel>(this IServiceCollection services, string windowName)
            where TWindow : class, IDialogWindow
            where TViewModel : class, IDialogAware
        => RegisterDialogWindow<TWindow, TViewModel>(services, windowName, null);


    public static IServiceCollection RegisterRouter(
       this IServiceCollection services,
       Action<IRouteMapper, IServiceProvider>? configureRoutes = null)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddSingletonWitAllMembers<IRouter>(sp =>
        {
            var router = ActivatorUtilities.CreateInstance<Router>(sp);
            configureRoutes?.Invoke(router, sp);
            return router;
        });

        return services;
    }

}