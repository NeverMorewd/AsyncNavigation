using AsyncNavigation.Core;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using System;
using System.Threading.Tasks;

namespace AsyncNavigation.WinUI;

public partial class ItemsRegion : RegionBase<ItemsRegion, ItemsControl>
{
    public ItemsRegion(string name,
        ItemsControl itemsControl, 
        IServiceProvider serviceProvider, 
        bool? useCache) : base(name, itemsControl, serviceProvider)
    {
        EnableViewCache = useCache ?? false;
        IsSinglePageRegion = false;
    }
    public override NavigationPipelineMode NavigationPipelineMode
    {
        get => NavigationPipelineMode.RenderFirst;
    }

    protected override void InitializeOnRegionCreated(ItemsControl control)
    {
        base.InitializeOnRegionCreated(control);
        control.Tag = this;
        control.HorizontalContentAlignment = Microsoft.UI.Xaml.HorizontalAlignment.Stretch;
        control.VerticalContentAlignment = Microsoft.UI.Xaml.VerticalAlignment.Stretch;
        control.SetBinding(ItemsControl.ItemsSourceProperty,
            new Binding
            {
                Path = new Microsoft.UI.Xaml.PropertyPath(nameof(RegionContext.Items)),
                Source = _context
            });

        if (control is Selector selector)
            selector.SetBinding(Selector.SelectedItemProperty,
            new Binding
            {
                Path = new Microsoft.UI.Xaml.PropertyPath(nameof(RegionContext.Selected)),
                Source = _context,
                Mode = BindingMode.TwoWay
            });

        control.ItemTemplate = XamlTemplateHelper.CreateIndicatorHostTemplate();
    }

    public override void Dispose()
    {
        base.Dispose();
        _context.Clear();
    }
    public override Task ProcessActivateAsync(NavigationContext navigationContext)
    {
        if (!_context.Items.Contains(navigationContext))
            _context.Items.Add(navigationContext);
        _context.Selected = navigationContext;

        return Task.CompletedTask;
    }

    public override Task ProcessDeactivateAsync(NavigationContext? navigationContext)
    {
        var target = navigationContext ?? _context.Selected;
        if (target == null)
            return Task.CompletedTask;
        _ = _context.Items.Remove(target);
        return Task.CompletedTask;
    }
}
