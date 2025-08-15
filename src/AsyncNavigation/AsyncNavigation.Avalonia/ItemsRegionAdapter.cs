using AsyncNavigation.Abstractions;
using Avalonia.Controls;

namespace AsyncNavigation.Avalonia;

public class ItemsRegionAdapter : RegionAdapterBase<ItemsControl>
{
    public override bool IsAdapted(ItemsControl control)
    {
        return base.IsAdapted(control);
    }
    public override IRegion CreateRegion(string name, ItemsControl control, IServiceProvider serviceProvider, bool? useCache = null)
    {
        return new ItemsRegion(name, control, serviceProvider, useCache);
    }
}
