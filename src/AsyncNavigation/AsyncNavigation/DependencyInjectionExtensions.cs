using AsyncNavigation.Abstractions;

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

}
