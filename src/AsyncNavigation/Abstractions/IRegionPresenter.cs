using AsyncNavigation.Core;

namespace AsyncNavigation.Abstractions;

public interface IRegionPresenter
{
    bool EnableViewCache { get; }
    bool IsSinglePageRegion { get; }
    NavigationPipelineMode NavigationPipelineMode { get; }
    Task ProcessActivateAsync(NavigationContext navigationContext);
    Task ProcessDeactivateAsync(NavigationContext? navigationContext);
}
