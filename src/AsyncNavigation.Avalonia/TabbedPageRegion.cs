using AsyncNavigation.Abstractions;
using AsyncNavigation.Core;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Microsoft.Extensions.DependencyInjection;

namespace AsyncNavigation.Avalonia;

public class TabbedPageRegion : RegionBase<TabbedPageRegion, TabbedPage>, IRegionPlacementParticipant
{
    private readonly IIconResolver<Control> _iconResolver;
    public TabbedPageRegion(string name,
        TabbedPage tabbedPage,
        IServiceProvider serviceProvider,
        bool? useCache) : base(name, tabbedPage, serviceProvider)
    {
        EnableViewCache = useCache ?? false;
        IsSinglePageRegion = false;
        _iconResolver = serviceProvider.GetRequiredService<IIconResolver<Control>>();
    }
    public override NavigationPipelineMode NavigationPipelineMode
    {
        get => NavigationPipelineMode.ResolveFirst;
    }

    protected override void InitializeOnRegionCreated(TabbedPage control)
    {
        base.InitializeOnRegionCreated(control);
        control.Tag = this;
        control.Bind(TabbedPage.ItemsSourceProperty,
            new Binding(nameof(RegionContext.Items)) { Source = _context });

        control.Bind(TabbedPage.CurrentPageProperty,
            new Binding(nameof(RegionContext.Selected)) { Source = _context, Mode = BindingMode.TwoWay });

        control.Bind(TabbedPage.HeaderProperty, new Binding(nameof(RegionContext.Selected)) { Source = _context, Mode = BindingMode.TwoWay });
        control.PageTemplate = new FuncDataTemplate<NavigationContext>((context, _) =>
        {
            var page = new ContentPage
            {
                Content = context?.IndicatorHost.Value?.Host,
            };
            if (context!.TryResolveNavigationAware(out var aware) && aware is INavigationMetadata metadata)
            {
                page.Icon = metadata;
                page.IconTemplate = new FuncDataTemplate<INavigationMetadata>((meta, _) => 
                {
                    return _iconResolver.Resolve(meta.Icon);
                });
                page.Header = metadata.Title;
            }
            return page;
            
        });
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

    public RegionPlacementItem Capture(Guid? navigationId = null)
    {
        var context = navigationId.HasValue
            ? _context.Items.FirstOrDefault(item => item.NavigationId == navigationId.Value)
            : _context.Selected;
        if (context is null)
            throw new InvalidOperationException($"Region '{Name}' does not contain the requested navigation item.");

        return new RegionPlacementItem(context, _context.Items.IndexOf(context),
            ReferenceEquals(_context.Selected, context));
    }

    public void Detach(RegionPlacementItem item)
    {
        ArgumentNullException.ThrowIfNull(item);
        var index = _context.Items.IndexOf(item.Context);
        if (index < 0)
            throw new InvalidOperationException($"The navigation item is not attached to region '{Name}'.");

        var wasSelected = ReferenceEquals(_context.Selected, item.Context);
        _context.Items.RemoveAt(index);
        if (wasSelected)
            _context.Selected = _context.Items.Count == 0 ? null : _context.Items[Math.Min(index, _context.Items.Count - 1)];
    }

    public void Attach(RegionPlacementItem item, bool activate = true)
    {
        ArgumentNullException.ThrowIfNull(item);
        if (_context.Items.Contains(item.Context))
            throw new InvalidOperationException($"The navigation item is already attached to region '{Name}'.");

        _context.Items.Insert(Math.Clamp(item.Index, 0, _context.Items.Count), item.Context);
        if (activate || item.WasSelected)
            _context.Selected = item.Context;
    }

}
