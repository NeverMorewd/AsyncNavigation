using AsyncNavigation.Core;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;

namespace AsyncNavigation.Wpf;

public class TabRegion : RegionBase<TabRegion>
{
    private readonly TabControl _tabControl;
    public TabRegion(string name, 
        TabControl control, 
        IServiceProvider serviceProvider, 
        bool? useCache = null) : base(name, serviceProvider)
    {
        ArgumentNullException.ThrowIfNull(control);
        ArgumentNullException.ThrowIfNull(serviceProvider);
        _tabControl = control;

        _tabControl.SetBinding(ItemsControl.ItemsSourceProperty,
            new Binding(nameof(RegionContext.Items))
            {
                Source = _context
            });

        _tabControl.SetBinding(Selector.SelectedItemProperty,
            new Binding(nameof(RegionContext.Selected))
            {
                Source = _context,
                Mode = BindingMode.TwoWay
            });

        var dataTemplate = new DataTemplate
        {
            VisualTree = new FrameworkElementFactory(typeof(ContentPresenter))
        };
        dataTemplate.VisualTree.SetBinding(ContentPresenter.ContentProperty,
            new Binding("IndicatorHost.Value.Host"));

        _tabControl.ContentTemplate = dataTemplate;

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


