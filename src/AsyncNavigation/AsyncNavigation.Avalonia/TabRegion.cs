using AsyncNavigation.Core;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Data;

namespace AsyncNavigation.Avalonia;

public class TabRegion : RegionBase<TabRegion>
{
    private readonly TabControl _tabControl;
    private readonly ItemsRegionContext _context = new();
    public TabRegion(TabControl control, IServiceProvider serviceProvider, bool? useCache = null) : base(serviceProvider)
    {
        ArgumentNullException.ThrowIfNull(control);
        ArgumentNullException.ThrowIfNull(serviceProvider);
        _tabControl = control;
        _tabControl.Bind(
           ItemsControl.ItemsSourceProperty,
           new Binding(nameof(ItemsRegionContext.Items)) { Source = _context });

        _tabControl.Bind(
            SelectingItemsControl.SelectedItemProperty,
            new Binding(nameof(ItemsRegionContext.Selected)) { Source = _context, Mode = BindingMode.TwoWay });
        _tabControl.ContentTemplate = new FuncDataTemplate<NavigationContext>((context, _) =>
        {
            return context?.Indicator.Value?.IndicatorControl as Control;
        });

        EnableViewCache = useCache ?? false;
    }
    public override bool EnableViewCache { get; }
    public override bool IsSinglePageRegion => false;

    public override void Dispose()
    {
        GC.SuppressFinalize(this);
        _context.Clear();
    }


    public override void ProcessActivate(NavigationContext navigationContext)
    {
        var hit = _context.Items.FirstOrDefault(t => t.Equals(navigationContext));
        if (hit != null)
        {
            _context.Selected = hit;
        }
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
        if (!_context.Items.Contains(navigationContext))
            _context.Items.Add(navigationContext);

        ProcessActivate(navigationContext);
    }
}


