using AsyncNavigation.Abstractions;
using Avalonia.Controls;

namespace AsyncNavigation.Avalonia;

public class ItemsRegionAdapter : RegionAdapterBase<ItemsControl>
{
    public override IRegion CreateRegion(string name, ItemsControl control, IServiceProvider serviceProvider, bool? useCache)
    {
        return new ItemsRegion(name, control, serviceProvider, useCache);
    }
}
