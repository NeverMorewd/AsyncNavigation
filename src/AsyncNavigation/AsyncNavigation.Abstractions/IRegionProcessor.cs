using AsyncNavigation.Core;

namespace AsyncNavigation.Abstractions;

public interface IRegionProcessor
{
    bool EnableViewCache { get; }
    bool IsSinglePageRegion { get; }
    void ProcessActivate(NavigationContext navigationContext);
    void ProcessDeactivate(NavigationContext navigationContext);
}
