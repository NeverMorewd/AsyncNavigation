using AsyncNavigation.Abstractions;
using Avalonia.Controls;

namespace AsyncNavigation.Avalonia;

public class ItemsControlAdapter : RegionAdapterBase<ItemsControl>
{
    public override IRegion CreateRegion(string name, ItemsControl control, IServiceProvider serviceProvider)
    {
        return new ItemsRegion(name, control, serviceProvider);
    }
}
