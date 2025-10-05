using AsyncNavigation.Abstractions;

namespace AsyncNavigation.Tests.Mocks;

internal class RegionManager : RegionManagerBase
{
    public RegionManager(IRegionFactory regionFactory, IServiceProvider serviceProvider) 
        : base(regionFactory, serviceProvider)
    {

    }
}
