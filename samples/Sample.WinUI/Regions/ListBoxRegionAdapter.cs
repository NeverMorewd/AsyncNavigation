using AsyncNavigation;
using AsyncNavigation.Abstractions;
using AsyncNavigation.WinUI;
using Microsoft.UI.Xaml.Controls;

namespace Sample.WinUI.Regions;

internal sealed class ListBoxRegionAdapter : RegionAdapterBase<ListBox>
{
    public override uint Priority => 100;

    public override IRegion CreateRegion(string name, ListBox control, IServiceProvider serviceProvider, bool? useCache)
        => new ListBoxRegion(name, control, serviceProvider, useCache);
}
