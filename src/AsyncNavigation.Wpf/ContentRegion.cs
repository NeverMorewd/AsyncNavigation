using AsyncNavigation.Core;
using AsyncNavigation.Abstractions;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace AsyncNavigation.Wpf;

public class ContentRegion : RegionBase<ContentRegion, ContentControl>, IRegionPlacementParticipant
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
        control.SetBinding(ContentControl.ContentProperty,
            new Binding(nameof(RegionContext.Selected))
            {
                Source = _context,
                Mode = BindingMode.TwoWay
            });

        var dataTemplate = new DataTemplate();
        var factory = new FrameworkElementFactory(typeof(ContentPresenter));
        factory.SetBinding(ContentPresenter.ContentProperty,
            new Binding("IndicatorHost.Value.Host")
            {
                FallbackValue = null
            });
        dataTemplate.VisualTree = factory;

        control.ContentTemplate = dataTemplate;
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

    public RegionPlacementItem Capture(Guid? navigationId = null)
    {
        var context = _context.Selected;
        if (context is null || navigationId.HasValue && context.NavigationId != navigationId.Value)
            throw new InvalidOperationException($"Region '{Name}' does not contain the requested navigation item.");

        return new RegionPlacementItem(context, 0, true);
    }

    public void Detach(RegionPlacementItem item)
    {
        ArgumentNullException.ThrowIfNull(item);
        if (!ReferenceEquals(_context.Selected, item.Context))
            throw new InvalidOperationException($"The navigation item is not attached to region '{Name}'.");
        _context.Selected = null;
    }

    public void Attach(RegionPlacementItem item, bool activate = true)
    {
        ArgumentNullException.ThrowIfNull(item);
        if (_context.Selected is not null)
            throw new InvalidOperationException($"Region '{Name}' already contains a navigation item.");
        _context.Selected = item.Context;
    }
}
