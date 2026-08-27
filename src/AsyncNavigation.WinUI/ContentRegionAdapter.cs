using AsyncNavigation.Abstractions;
using Microsoft.UI.Xaml.Controls;
using System;

namespace AsyncNavigation.WinUI;

public class ContentRegionAdapter : RegionAdapterBase<ContentControl>
{
    public override IRegion CreateRegion(string name, 
        ContentControl control, 
        IServiceProvider serviceProvider, 
        bool? useCache)
    {
        return new ContentRegion(name, control, serviceProvider, useCache);
    }
}
