using AsyncNavigation.Core;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Data;
using System;
using System.Threading.Tasks;

namespace AsyncNavigation.WinUI;

public partial class ContentRegion : RegionBase<ContentRegion, ContentControl>
{
    public ContentRegion(string name, 
        ContentControl contentControl, 
        IServiceProvider serviceProvider, 
        bool? useCache) : base(name, contentControl, serviceProvider)
    {
        EnableViewCache = useCache ?? true;
        IsSinglePageRegion = true;
    }
    public override NavigationPipelineMode NavigationPipelineMode
    {
        get => NavigationPipelineMode.RenderFirst;
    }

    protected override void InitializeOnRegionCreated(ContentControl control)
    {
        base.InitializeOnRegionCreated(control);
        control.Tag = this;
        control.SetBinding(ContentControl.ContentProperty, new Binding
        {
            Path = new Microsoft.UI.Xaml.PropertyPath(nameof(RegionContext.Selected)),
            Source = _context,
            Mode = BindingMode.TwoWay
        });
        control.ContentTemplate = XamlTemplateHelper.CreateIndicatorHostTemplate();
    }
    public override void Dispose()
    {
        base.Dispose();
        _context.Selected = null;
        RegionControlAccessor.ExecuteOn(control =>
        {
            control.Content = null;
        });
    }

    public override Task ProcessActivateAsync(NavigationContext navigationContext)
    {
        _context.Selected = navigationContext;
        return Task.CompletedTask;
    }

    public override Task ProcessDeactivateAsync(NavigationContext? navigationContext)
    {
        _context.Selected = null;
        return Task.CompletedTask;
    }
}
