using AsyncNavigation.Core;

namespace AsyncNavigation.Abstractions;

public interface IRegionProcessor
{
    bool ShouldCheckSameNameViewCache { get; }
    bool AllowMultipleViews { get; }
    void ProcessActivate(NavigationContext navigationContext);
    void ProcessDeactivate(NavigationContext navigationContext);
}
