using AsyncNavigation.Abstractions;
using Microsoft.UI.Xaml.Controls;
using System;

namespace AsyncNavigation.WinUI;

public sealed class NavigationViewRegionAdapter : RegionAdapterBase<NavigationView>
{
    public override IRegion CreateRegion(
        string name,
        NavigationView control,
        IServiceProvider serviceProvider,
        bool? useCache)
        => new NavigationViewRegion(name, control, serviceProvider, useCache);
}
