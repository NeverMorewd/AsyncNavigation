using AsyncNavigation.Avalonia.Floating;
using AsyncNavigation.Floating;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Microsoft.Extensions.DependencyInjection;

public static class FloatingDependencyInjectionExtensions
{
    public static IServiceCollection AddFloatingSupport(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);
        services.TryAddSingleton<IFloatingWindowHostFactory, AvaloniaFloatingWindowHostFactory>();
        return services.AddFloatingSupportCore();
    }
}
