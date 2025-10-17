using AsyncNavigation;
using AsyncNavigation.Abstractions;
using AsyncNavigation.Core;
using AsyncNavigation.Wpf;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Diagnostics.CodeAnalysis;

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
    /// Register <see cref="IJobScheduler"/> as a singleton or transient service depending on
    /// <see cref="NavigationOptions.NavigationJobScope"/>.
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
        return serviceDescriptors
            .RegisterNavigationFramework(navigationOptions)
            .RegisterRegionAdapter<ContentRegionAdapter>()
            .RegisterRegionAdapter<ItemsRegionAdapter>()
            .RegisterRegionAdapter<TabRegionAdapter>()
            .AddTransient<IInnerRegionIndicatorHost, InnerIndicatorHost>()
            .AddSingleton<RegionManager>()
            .AddSingleton<IRegionManager>(sp => sp.GetRequiredService<RegionManager>())
            .RegisterDialogWindow<DefaultDialogContainer>(NavigationConstants.DEFAULT_DIALOG_WINDOW_KEY)
            .AddSingleton<IPlatformService, PlatformService>();
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
    public static IServiceCollection RegisterRegionAdapter<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] T>(this IServiceCollection serviceDescriptors)
        where T : class, IRegionAdapter
    {
        return serviceDescriptors.AddSingleton<IRegionAdapter, T>();
    }

    /// <summary>
    /// Registers an implementation of <see cref="IInnerIndicatorProvider"/> in the DI container as a transient service.
    /// </summary>
    /// <typeparam name="T">
    /// The concrete type that implements <see cref="IInnerIndicatorProvider"/>.
    /// </typeparam>
    /// <param name="serviceDescriptors">
    /// The <see cref="IServiceCollection"/> to add the service descriptor to.
    /// </param>
    /// <returns>
    /// The updated <see cref="IServiceCollection"/> for method chaining.
    /// </returns>
    /// <remarks>
    /// This method uses <c>TryAddTransient</c>, which means:
    /// <list type="bullet">
    /// <item>
    /// If no service of type <see cref="IInnerIndicatorProvider"/> is registered, it will add <typeparamref name="T"/>.
    /// </item>
    /// <item>
    /// If a service of type <see cref="IInnerIndicatorProvider"/> is already registered, this call will be ignored.
    /// </item>
    /// </list>
    /// <para>
    /// ⚠ Important: This method should be called only once. Subsequent calls will not override the previous registration.
    /// If you need to override an existing registration, consider using <c>Replace</c> from
    /// <see cref="ServiceCollectionDescriptorExtensions"/>.
    /// </para>
    /// </remarks>
    public static IServiceCollection RegisterInnerIndicatorProvider<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] T>(this IServiceCollection serviceDescriptors)
        where T : class, IInnerIndicatorProvider
    {
        serviceDescriptors.TryAddTransient<IInnerIndicatorProvider, T>();
        return serviceDescriptors;
    }
}
