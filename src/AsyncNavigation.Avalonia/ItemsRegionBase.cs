using AsyncNavigation.Core;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Data;

namespace AsyncNavigation.Avalonia;

public abstract class ItemsRegionBase<TRegion, TItemsControl>
    : RegionBase<TRegion, TItemsControl>
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


    public override void ProcessActivate(NavigationContext navigationContext)
    {
        if (!_context.Items.Contains(navigationContext))
            _context.Items.Add(navigationContext);

        _context.Selected = navigationContext;

        RegionControlAccessor.ExecuteOn(control =>
        {
            control.ScrollIntoView(navigationContext);
        });
    }

    public override void ProcessDeactivate(NavigationContext? navigationContext)
    {
        var target = navigationContext ?? _context.Selected;
        if (target == null)
            return;

        _ = _context.Items.Remove(target);
    }

    public override void Dispose()
    {
        base.Dispose();
        _context.Clear();
    }
}
