using AsyncNavigation.Core;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;

namespace AsyncNavigation.Wpf;

public class ItemsRegion : RegionBase<ItemsRegion>
{
    private readonly ItemsControl _itemsControl;
    public ItemsRegion(ItemsControl itemsControl, IServiceProvider serviceProvider, bool? useCache) : base(serviceProvider)
    {
        ArgumentNullException.ThrowIfNull(itemsControl);
        ArgumentNullException.ThrowIfNull(serviceProvider);

        _itemsControl = itemsControl;

        _itemsControl.SetBinding(ItemsControl.ItemsSourceProperty, 
            new Binding(nameof(RegionContext.Items))
            {
                Source = _context
            });

        _itemsControl.SetBinding(Selector.SelectedItemProperty, 
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

        _itemsControl.ItemTemplate = dataTemplate;


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
