using AsyncNavigation.Abstractions;

namespace AsyncNavigation;

internal sealed class RegionNavigationServiceFactory : IRegionNavigationServiceFactory
{
    private readonly IServiceProvider _serviceProvider;

    public RegionNavigationServiceFactory(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public IRegionNavigationService<T> Create<T>(T region) where T : IRegionPresenter
    {
        return new RegionNavigationService<T>(region, _serviceProvider);
    }
}
