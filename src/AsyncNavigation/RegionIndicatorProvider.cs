using AsyncNavigation.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace AsyncNavigation;

public class RegionIndicatorProvider : IRegionIndicatorProvider
{
    private readonly IServiceProvider _serviceProvider;

    public RegionIndicatorProvider(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }
    public IRegionIndicator GetIndicator(string regionName)
    {
        return _serviceProvider.GetRequiredKeyedService<IRegionIndicator>(regionName);
    }

    public bool HasIndicator(string regionName)
    {
        return _serviceProvider.GetKeyedService<IRegionIndicator>(regionName) != null;
    }
}
