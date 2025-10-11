using AsyncNavigation.Core;

namespace AsyncNavigation.Abstractions;

public interface IRegionPresenter
{
    bool EnableViewCache { get; }
    bool IsSinglePageRegion { get; }
    NavigationPipelineMode NavigationPipelineMode { get; }
    void ProcessActivate(NavigationContext navigationContext);
    void ProcessDeactivate(NavigationContext? navigationContext);
}
