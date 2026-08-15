using AsyncNavigation.Abstractions;
using Microsoft.UI.Xaml.Controls;
using System;

namespace AsyncNavigation.WinUI;

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
