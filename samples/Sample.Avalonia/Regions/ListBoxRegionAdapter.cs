using AsyncNavigation;
using AsyncNavigation.Abstractions;
using Avalonia.Controls;
using System;

namespace Sample.Avalonia.Regions;

internal class ListBoxRegionAdapter : RegionAdapterBase<ListBox>
{
    public override IRegion CreateRegion(ListBox control, IServiceProvider serviceProvider, bool? useCache = null)
    {
        return new ListBoxRegion(control, serviceProvider, useCache);
    }
}
