using AsyncNavigation;
using AsyncNavigation.Abstractions;
using System.Windows.Controls;

namespace Sample.Wpf.Regions;

internal class ListBoxRegionAdapter : RegionAdapterBase<ListBox>
{
    public override IRegion CreateRegion(string name, ListBox control, IServiceProvider serviceProvider, bool? useCache)
    {
        return new ListBoxRegion(name, control, serviceProvider, useCache);
    }
}
