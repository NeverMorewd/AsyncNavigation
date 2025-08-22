namespace AsyncNavigation.Abstractions;

public interface IRegionPresenter
{
    bool EnableViewCache { get; }
    bool IsSinglePageRegion { get; }
    void ProcessActivate(NavigationContext navigationContext);
    void RenderIndicator(NavigationContext navigationContext);
    void ProcessDeactivate(NavigationContext navigationContext);
}
