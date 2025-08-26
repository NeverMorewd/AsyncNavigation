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
        IsSinglePageRegion = true;
    }

    public override void Dispose()
    {
        base.Dispose();
        _contentControl.Content = null;
    }

    public override void RenderIndicator(NavigationContext navigationContext)
    {
        _contentControl.Content = navigationContext.Indicator.Value!.IndicatorControl;
    }

    public override void ProcessActivate(NavigationContext navigationContext)
    {
        _contentControl.Content = navigationContext.Indicator.Value!.IndicatorControl;
    }

    public override void ProcessDeactivate(NavigationContext navigationContext)
    {
        _contentControl.Content = null;
    }
}
