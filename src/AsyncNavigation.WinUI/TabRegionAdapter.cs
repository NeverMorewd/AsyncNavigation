using AsyncNavigation.Abstractions;
using Microsoft.UI.Xaml.Controls;
using System;

namespace AsyncNavigation.WinUI;

public class TabRegionAdapter : RegionAdapterBase<TabView>
{
    public override bool IsAdapted(TabView control)
    {
        return base.IsAdapted(control);
    }
    public override IRegion CreateRegion(string name, TabView control, IServiceProvider serviceProvider, bool? useCache = null)
    {
       return new TabRegion(name, control, serviceProvider, useCache);
    }
}
