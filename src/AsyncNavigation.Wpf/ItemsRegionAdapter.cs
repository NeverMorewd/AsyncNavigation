using AsyncNavigation.Abstractions;
using System.Windows.Controls;

namespace AsyncNavigation.Wpf;

public class ItemsRegionAdapter : RegionAdapterBase<ItemsControl>
{
    public override bool IsAdapted(ItemsControl control)
    {
        return base.IsAdapted(control);
    }
    public override IRegion CreateRegion(ItemsControl control, IServiceProvider serviceProvider, bool? useCache = null)
    {
        return new ItemsRegion(control, serviceProvider, useCache);
    }
}
