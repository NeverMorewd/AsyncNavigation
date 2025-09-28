using AsyncNavigation.Abstractions;
using Avalonia.Controls;

namespace AsyncNavigation.Avalonia
{
    public class TabRegionAdapter : RegionAdapterBase<TabControl>
    {
        public override bool IsAdapted(TabControl control)
        {
            return base.IsAdapted(control);
        }
        public override IRegion CreateRegion(string name, TabControl control, IServiceProvider serviceProvider, bool? useCache)
        {
           return new TabRegion(name, control, serviceProvider, useCache);
        }
    }
}
