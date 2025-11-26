using AsyncNavigation.Abstractions;
using Avalonia.Controls.Primitives;

namespace AsyncNavigation.Avalonia;

public class SelectingItemsRegionAdapter : RegionAdapterBase<SelectingItemsControl>
{
    public override uint Priority => 1;
    public override IRegion CreateRegion(string name, SelectingItemsControl control, IServiceProvider serviceProvider, bool? useCache)
    {
        return new SelectingItemsRegion(name, control, serviceProvider, useCache);
    }
}
