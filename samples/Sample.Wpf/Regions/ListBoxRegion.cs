using AsyncNavigation;
using AsyncNavigation.Wpf;
using System.Windows.Controls;

namespace Sample.Wpf.Regions;

public class ListBoxRegion : ItemsRegion
{
    private readonly ListBox _listBox;
    public ListBoxRegion(string name,
        ListBox listBox, 
        IServiceProvider serviceProvider, 
        bool? useCache) : base(name, listBox, serviceProvider, useCache)
    {
        _listBox = listBox;
    }
    public override void ProcessActivate(NavigationContext navigationContext)
    {
        base.ProcessActivate(navigationContext);
        _listBox.ScrollIntoView(navigationContext);
    }
}
