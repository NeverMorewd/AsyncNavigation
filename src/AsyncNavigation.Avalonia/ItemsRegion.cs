using AsyncNavigation.Core;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Data;

namespace AsyncNavigation.Avalonia;

public class ItemsRegion : RegionBase<ItemsRegion, ItemsControl>
{
    public ItemsRegion(string name, 
        ItemsControl itemsControl, 
        IServiceProvider serviceProvider, 
        bool? useCache) : base(name, itemsControl, serviceProvider)
    {
        ArgumentNullException.ThrowIfNull(itemsControl);
        ArgumentNullException.ThrowIfNull(serviceProvider);

        RegionControlAccessor.ExecuteOn(control =>
        {
            control.ItemTemplate = new FuncDataTemplate<NavigationContext>((context, np) =>
            {
                return context?.IndicatorHost.Value?.Host as Control;
            });

            control.Bind(
               ItemsControl.ItemsSourceProperty,
               new Binding(nameof(RegionContext.Items)) { Source = _context });

            control.Bind(
                SelectingItemsControl.SelectedItemProperty,
                new Binding(nameof(RegionContext.Selected)) { Source = _context, Mode = BindingMode.TwoWay });
        });
        EnableViewCache = useCache ?? false;
        IsSinglePageRegion = false;
    }

    public override void Dispose()
    {
        base.Dispose();
        _context.Clear();
    }
    public override void ProcessActivate(NavigationContext navigationContext)
    {
        if (!_context.Items.Contains(navigationContext))
            _context.Items.Add(navigationContext);

        _context.Selected = navigationContext;

        RegionControlAccessor.ExecuteOn(control =>
        {
            control.ScrollIntoView(navigationContext);
        });
    }
    public override void RenderIndicator(NavigationContext navigationContext)
    {
        ProcessActivate(navigationContext);
    }

    public override void ProcessDeactivate(NavigationContext navigationContext)
    {
        _context.Items.Remove(navigationContext);
    }
}
