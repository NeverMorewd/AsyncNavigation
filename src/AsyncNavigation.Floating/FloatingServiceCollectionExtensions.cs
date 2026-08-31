using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.ComponentModel;

namespace AsyncNavigation.Floating;

[EditorBrowsable(EditorBrowsableState.Never)]
public static class FloatingServiceCollectionExtensions
{
    public static IServiceCollection AddFloatingSupportCore(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);
        services.TryAddSingleton<IViewPlacementService, ViewPlacementService>();
        return services;
    }
}
