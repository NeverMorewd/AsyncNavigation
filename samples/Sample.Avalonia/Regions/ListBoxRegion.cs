using AsyncNavigation.Avalonia;
using Avalonia.Controls;
using System;

namespace Sample.Avalonia.Regions;

public class ListBoxRegion : SelectingItemsRegion
{
    private readonly ListBox _listBox;
    public ListBoxRegion(string name,
        ListBox listBox, 
        IServiceProvider serviceProvider, 
        bool? useCache) : base(name, listBox, serviceProvider, useCache)
    {
        _listBox = listBox;
        _listBox.AutoScrollToSelectedItem = true;
    }
}
