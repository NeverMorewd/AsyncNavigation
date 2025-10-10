using AsyncNavigation.Core;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;

namespace AsyncNavigation.Wpf;

public class TabRegion : RegionBase<TabRegion, TabControl>
{
    public TabRegion(string name, 
        TabControl tabControl, 
        IServiceProvider serviceProvider, 
        bool? useCache = null) : base(name, tabControl, serviceProvider)
    {
        ArgumentNullException.ThrowIfNull(tabControl);
        ArgumentNullException.ThrowIfNull(serviceProvider);

        RegionControlAccessor.ExecuteOn(control => 
        {
            control.Tag = this;
            control.SetBinding(ItemsControl.ItemsSourceProperty,
                new Binding(nameof(RegionContext.Items))
                {
                    Source = _context
                });

            control.SetBinding(Selector.SelectedItemProperty,
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

            control.ContentTemplate = dataTemplate;
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
    }

    public override void ProcessDeactivate(NavigationContext? navigationContext)
    {
        var target = navigationContext ?? _context.Selected;
        if (target == null)
            return;

        _ = _context.Items.Remove(target);
    }
}


