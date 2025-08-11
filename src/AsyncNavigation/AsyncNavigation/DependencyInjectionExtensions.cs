using AsyncNavigation.Abstractions;

namespace Microsoft.Extensions.DependencyInjection;

public static class DependencyInjectionExtensions
{
    public static IServiceCollection RegisterNavigation<TView, TViewModel>(this IServiceCollection serviceDescriptors, string viewKey)
        where TView : class, IView
        where TViewModel : class, INavigationAware
    {
        ArgumentNullException.ThrowIfNull(serviceDescriptors);
        ArgumentException.ThrowIfNullOrWhiteSpace(viewKey);

        serviceDescriptors
            .AddTransient<TViewModel>()
            .AddTransient<TView>()
            .AddKeyedTransient<IView>(viewKey, (sp, key) => sp.GetRequiredService<TView>())
            .AddKeyedTransient<INavigationAware>(viewKey, (sp, key) => sp.GetRequiredService<TViewModel>());

        if (typeof(TViewModel).IsAssignableTo(typeof(IDialogAware)))
        {
            serviceDescriptors.AddKeyedTransient(viewKey, (sp, key) =>
                (IDialogAware)sp.GetRequiredService<TViewModel>());
        }
        return serviceDescriptors;
    }
}
