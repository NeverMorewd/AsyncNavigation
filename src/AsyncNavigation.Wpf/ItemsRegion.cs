using AsyncNavigation.Core;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;

namespace AsyncNavigation.Wpf;

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

            control.ItemTemplate = dataTemplate;
        });

        EnableViewCache = useCache ?? false;
        IsSinglePageRegion = false;
    }
    public override NavigationPipelineMode NavigationPipelineMode
    {
        get => NavigationPipelineMode.RenderFirst;
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
