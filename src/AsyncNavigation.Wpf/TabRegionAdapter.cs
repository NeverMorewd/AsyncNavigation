using AsyncNavigation.Abstractions;
using System.Windows.Controls;

namespace AsyncNavigation.Wpf;

public class TabRegionAdapter : RegionAdapterBase<TabControl>
{
    public override bool IsAdapted(TabControl control)
    {
        return base.IsAdapted(control);
    }
    public override IRegion CreateRegion(TabControl control, IServiceProvider serviceProvider, bool? useCache = null)
    {
       return new TabRegion(control, serviceProvider, useCache);
    }
}
