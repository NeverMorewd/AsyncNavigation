using AsyncNavigation;
using Microsoft.UI.Xaml.Controls;

namespace Sample.WinUI.Regions;

public sealed class ListBoxRegion : AsyncNavigation.WinUI.ItemsRegion
{
    private readonly ListBox _listBox;

    public ListBoxRegion(string name, ListBox listBox, IServiceProvider serviceProvider, bool? useCache)
        : base(name, listBox, serviceProvider, useCache) => _listBox = listBox;

    public override async Task ProcessActivateAsync(NavigationContext navigationContext)
    {
        await base.ProcessActivateAsync(navigationContext);
        _listBox.ScrollIntoView(navigationContext);
    }
}
