using AsyncNavigation.Core;
using AsyncNavigation.Abstractions;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Threading;

namespace AsyncNavigation.Avalonia;

public abstract class ItemsRegionBase<TRegion, TItemsControl>
    : RegionBase<TRegion, TItemsControl>, IRegionPlacementParticipant
    where TRegion : ItemsRegionBase<TRegion, TItemsControl>
    where TItemsControl : ItemsControl
{
    protected ItemsRegionBase(
        string name,
        TItemsControl control,
        IServiceProvider serviceProvider,
        bool? useCache)
        : base(name, control, serviceProvider)
    {
        IsSinglePageRegion = false;
        EnableViewCache = useCache ?? false;
    }

    public override NavigationPipelineMode NavigationPipelineMode
        => NavigationPipelineMode.RenderFirst;

    protected override void InitializeOnRegionCreated(TItemsControl control)
    {
        base.InitializeOnRegionCreated(control);

        control.Tag = this;
        control.ItemTemplate = new FuncDataTemplate<NavigationContext>((context, _) =>
        {
            return context?.IndicatorHost.Value?.Host as Control;
        });

        control.Bind(
            ItemsControl.ItemsSourceProperty,
            new Binding(nameof(RegionContext.Items)) { Source = _context });
    }


    public override Task ProcessActivateAsync(NavigationContext navigationContext)
    {
        if (!_context.Items.Contains(navigationContext))
            _context.Items.Add(navigationContext);

        _context.Selected = navigationContext;
        // https://github.com/AvaloniaUI/Avalonia/issues/17347
        Dispatcher.UIThread.RunJobs(DispatcherPriority.Loaded);
        RegionControlAccessor.ExecuteOn(control =>
        {
            control.ScrollIntoView(navigationContext);
        });
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

    public override void Dispose()
    {
        base.Dispose();
        _context.Clear();
    }

    public RegionPlacementItem Capture(Guid? navigationId = null)
    {
        var context = navigationId.HasValue
            ? _context.Items.FirstOrDefault(item => item.NavigationId == navigationId.Value)
            : _context.Selected ?? _context.Items.LastOrDefault();
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
