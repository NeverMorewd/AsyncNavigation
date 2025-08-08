using AsyncNavigation.Core;

namespace AsyncNavigation.Abstractions;

public interface IRegionProcessor
{
    void ProcessActivate(NavigationContext navigationContext, object content);
    void ProcessDeactivate(NavigationContext navigationContext, object content);
}
