using AsyncNavigation.Abstractions;
using Avalonia.Controls;

namespace AsyncNavigation.Avalonia;

public class NavigationPageRegionAdapter : RegionAdapterBase<NavigationPage>
{
    public override IRegion CreateRegion(string name, NavigationPage control, IServiceProvider serviceProvider, bool? useCache)
    {
        return new NavigationPageRegion(name, control, serviceProvider, useCache);
    }
}
