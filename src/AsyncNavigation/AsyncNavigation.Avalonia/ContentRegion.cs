using Avalonia.Controls;

namespace AsyncNavigation.Avalonia;

public class ContentRegion : RegionBase<ContentRegion>
{
    private readonly ContentControl _contentControl;
    public ContentRegion(ContentControl contentControl, IServiceProvider serviceProvider, bool? useCache) : base(serviceProvider)
    {
        ArgumentNullException.ThrowIfNull(contentControl);
        ArgumentNullException.ThrowIfNull(serviceProvider);
        _contentControl = contentControl;
        EnableViewCache = useCache ?? true;
    }
    public override bool EnableViewCache { get; }
    public override bool IsSinglePageRegion => true;

    public override void Dispose()
    {
        GC.SuppressFinalize(this);
        _contentControl.Content = null;
    }

    protected virtual void OnRenderIndicator(NavigationContext navigationContext)
    {

    }
    protected virtual void OnProcessActivate(NavigationContext navigationContext)
    {

    }
    protected virtual void OnProcessDeactivate(NavigationContext navigationContext)
    {

    }

    public override void RenderIndicator(NavigationContext navigationContext)
    {
        _contentControl.Content = navigationContext.Indicator.Value!.IndicatorControl;
        OnRenderIndicator(navigationContext);
    }

    public override void ProcessActivate(NavigationContext navigationContext)
    {
        _contentControl.Content = navigationContext.Indicator.Value!.IndicatorControl;
        OnProcessActivate(navigationContext);
    }

    public override void ProcessDeactivate(NavigationContext navigationContext)
    {
        _contentControl.Content = null;
        OnProcessDeactivate(navigationContext);
    }
}
