using AsyncNavigation.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace AsyncNavigation;

public class SharedIndicatorProvider : IIndicatorProvider
{
    private readonly IServiceProvider _serviceProvider;

    public SharedIndicatorProvider(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }
    public IRegionIndicator GetIndicator()
    {
        return _serviceProvider.GetRequiredService<IRegionIndicator>();
    }

    public bool HasIndicator()
    {
        return _serviceProvider.GetService<IRegionIndicator>() != null;
    }
}
