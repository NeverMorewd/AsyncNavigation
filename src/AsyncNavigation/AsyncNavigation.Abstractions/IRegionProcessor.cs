using AsyncNavigation.Core;

namespace AsyncNavigation.Abstractions;

public interface IRegionProcessor
{
    bool EnableViewCache { get; }
    bool IsSinglePageRegion { get; }
    void ProcessActivate(NavigationContext navigationContext);
    void RenderIndicator(NavigationContext navigationContext, IRegionIndicator regionIndicator);
    void ProcessDeactivate(NavigationContext navigationContext);
}
