using AsyncNavigation.Core;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Data;

namespace AsyncNavigation.Avalonia;

public class TabRegion : RegionBase<TabRegion>
{
    private readonly TabControl _tabControl;
    public TabRegion(string name, 
        TabControl control, 
        IServiceProvider serviceProvider, 
        bool? useCache) : base(name, serviceProvider)
    {
        ArgumentNullException.ThrowIfNull(control);
        ArgumentNullException.ThrowIfNull(serviceProvider);
        _tabControl = control;

        _tabControl.Bind(
           ItemsControl.ItemsSourceProperty,
           new Binding(nameof(RegionContext.Items)) { Source = _context });

        _tabControl.Bind(
            SelectingItemsControl.SelectedItemProperty,
            new Binding(nameof(RegionContext.Selected)) { Source = _context, Mode = BindingMode.TwoWay });

        _tabControl.ContentTemplate = new FuncDataTemplate<NavigationContext>((context, _) =>
        {
            return context?.IndicatorHost.Value?.Host as Control;
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
    }

    public override void ProcessDeactivate(NavigationContext navigationContext)
    {
        var hit = _context.Items.FirstOrDefault(t => ReferenceEquals(t, navigationContext));
        if (hit != null)
        {
            bool wasSelected = ReferenceEquals(_context.Selected, hit);
            _context.Items.Remove(hit);
            if (wasSelected)
                _context.Selected = _context.Items.FirstOrDefault();
        }
    }

    public override void RenderIndicator(NavigationContext navigationContext)
    {
        ProcessActivate(navigationContext);
    }
}


