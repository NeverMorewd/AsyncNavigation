using AsyncNavigation.Core;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Data;

namespace AsyncNavigation.Avalonia;

public class TabRegion : RegionBase<TabRegion, TabControl>
{
    public TabRegion(string name,
        TabControl tabControl,
        IServiceProvider serviceProvider,
        bool? useCache) : base(name, tabControl, serviceProvider)
    {
        ArgumentNullException.ThrowIfNull(tabControl);
        ArgumentNullException.ThrowIfNull(serviceProvider);

        RegionControlAccessor.ExecuteOn(control => 
        {
            control.Tag = this;
            control.Bind(ItemsControl.ItemsSourceProperty,
                new Binding(nameof(RegionContext.Items)) { Source = _context });

            control.Bind(SelectingItemsControl.SelectedItemProperty,
                new Binding(nameof(RegionContext.Selected)) { Source = _context, Mode = BindingMode.TwoWay });

            control.ContentTemplate = new FuncDataTemplate<NavigationContext>((context, _) =>
            {
                return context?.IndicatorHost.Value?.Host as Control;
            });
        });

       
        EnableViewCache = useCache ?? false;
        IsSinglePageRegion = false;
    }
    public override NavigationPipelineMode NavigationPipelineMode
    {
        get => NavigationPipelineMode.ResolveFirst;
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

    public override void ProcessDeactivate(NavigationContext? navigationContext)
    {
        var target = navigationContext ?? _context.Selected;
        if (target == null)
            return;

        _ = _context.Items.Remove(target);
    }

}


