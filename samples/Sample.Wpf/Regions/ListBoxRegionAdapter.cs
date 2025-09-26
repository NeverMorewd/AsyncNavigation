using AsyncNavigation;
using AsyncNavigation.Abstractions;
using System.Windows.Controls;

namespace Sample.Wpf.Regions;

internal class ListBoxRegionAdapter : RegionAdapterBase<ListBox>
{
    public override IRegion CreateRegion(ListBox control, IServiceProvider serviceProvider, bool? useCache = null)
    {
        return new ListBoxRegion(control, serviceProvider, useCache);
    }
}
