using AsyncNavigation.Core;
using Avalonia.Controls.Primitives;
using Avalonia.Data;

namespace AsyncNavigation.Avalonia;

public class SelectingItemsRegion : ItemsRegionBase<SelectingItemsRegion, SelectingItemsControl>
{
    public SelectingItemsRegion(string name, 
        SelectingItemsControl selectingItemsControl, 
        IServiceProvider serviceProvider, 
        bool? useCache) : base(name, selectingItemsControl, serviceProvider, useCache)
    {

    }
    protected override void InitializeOnRegionCreated(SelectingItemsControl control)
    {
        base.InitializeOnRegionCreated(control);
        control.Bind(
            SelectingItemsControl.SelectedItemProperty,
            new Binding(nameof(RegionContext.Selected)) { Source = _context, Mode = BindingMode.TwoWay });
    }
}
