using AsyncNavigation;
using AsyncNavigation.Avalonia;
using Avalonia.Controls;
using System;

namespace Sample.Avalonia.Regions;

public class ListBoxRegion : ItemsRegion
{
    private readonly ListBox _listBox;
    public ListBoxRegion(ListBox listBox, 
        IServiceProvider serviceProvider, 
        bool? useCache) : base(listBox, serviceProvider, useCache)
    {
        _listBox = listBox;
        _listBox.AutoScrollToSelectedItem = true;
    }
    public override void ProcessActivate(NavigationContext navigationContext)
    {
        base.ProcessActivate(navigationContext);
    }
}
