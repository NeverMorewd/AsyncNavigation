using AsyncNavigation.Floating;
using AsyncNavigation.Wpf.Floating;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Microsoft.Extensions.DependencyInjection;

public static class FloatingDependencyInjectionExtensions
{
    public static IServiceCollection AddFloatingSupport(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);
        services.TryAddSingleton<IFloatingWindowHostFactory, WpfFloatingWindowHostFactory>();
        return services.AddFloatingSupportCore();
    }
}
