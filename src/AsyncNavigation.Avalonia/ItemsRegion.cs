using AsyncNavigation.Core;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Data;

namespace AsyncNavigation.Avalonia;

public class ItemsRegion : RegionBase<ItemsRegion>
{
    private readonly ItemsControl _itemsControl;
    private readonly ItemsRegionContext _context = new();
    public ItemsRegion(ItemsControl itemsControl, IServiceProvider serviceProvider, bool? useCache) : base(serviceProvider)
    {
        ArgumentNullException.ThrowIfNull(itemsControl);
        ArgumentNullException.ThrowIfNull(serviceProvider);

        _itemsControl = itemsControl;

        _itemsControl.ItemTemplate = new FuncDataTemplate<NavigationContext>((context, np) =>
        {
            return context?.Indicator.Value?.IndicatorControl as Control;
        });

        _itemsControl.Bind(
           ItemsControl.ItemsSourceProperty,
           new Binding(nameof(ItemsRegionContext.Items)) { Source = _context });

        _itemsControl.Bind(
            SelectingItemsControl.SelectedItemProperty,
            new Binding(nameof(ItemsRegionContext.Selected)) { Source = _context, Mode = BindingMode.TwoWay });

        EnableViewCache = useCache ?? false;
        IsSinglePageRegion = false;
    }

    public override void Dispose()
    {
        base.Dispose();
        _itemsControl.DataContext = null;
        _context.Clear();
    }
    public override void ProcessActivate(NavigationContext navigationContext)
    {
        _context.Selected = navigationContext;
        _itemsControl.ScrollIntoView(navigationContext);
    }
    public override void RenderIndicator(NavigationContext navigationContext)
    {
        if (!_context.Items.Contains(navigationContext))
            _context.Items.Add(navigationContext);
        ProcessActivate(navigationContext);
    }

    public override void ProcessDeactivate(NavigationContext navigationContext)
    {
        _context.Items.Remove(navigationContext);
    }
}
