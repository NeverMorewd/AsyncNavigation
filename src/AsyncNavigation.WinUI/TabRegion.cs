using AsyncNavigation.Core;

using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Data;
using System;
using System.Threading.Tasks;

namespace AsyncNavigation.WinUI;

public class TabRegion : RegionBase<TabRegion, TabView>
{
    public TabRegion(string name, 
        TabView tabControl,
        IServiceProvider serviceProvider, 
        bool? useCache = null) : base(name, tabControl, serviceProvider)
    {
        EnableViewCache = useCache ?? false;
        IsSinglePageRegion = false;
    }
    public override NavigationPipelineMode NavigationPipelineMode
    {
        get => NavigationPipelineMode.ResolveFirst;
    }

    protected override void InitializeOnRegionCreated(TabView control)
    {
        base.InitializeOnRegionCreated(control);
        control.Tag = this;
        control.SetBinding(TabView.TabItemsSourceProperty,
            new Binding
            {
                Path = new Microsoft.UI.Xaml.PropertyPath(nameof(RegionContext.Items)),
                Source = _context
            });

        control.SetBinding(TabView.SelectedItemProperty,
            new Binding
            {
                Path = new Microsoft.UI.Xaml.PropertyPath(nameof(RegionContext.Selected)),
                Source = _context,
                Mode = BindingMode.TwoWay
            });

        control.TabItemTemplate = XamlTemplateHelper.CreateTabItemTemplate();
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


