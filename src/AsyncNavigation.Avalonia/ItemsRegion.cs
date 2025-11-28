using Avalonia.Controls;

namespace AsyncNavigation.Avalonia;

public class ItemsRegion : ItemsRegionBase<ItemsRegion, ItemsControl>
{
    public ItemsRegion(string name, 
        ItemsControl itemsControl, 
        IServiceProvider serviceProvider, 
        bool? useCache) : base(name, itemsControl, serviceProvider, useCache)
    {
        
    }
}



