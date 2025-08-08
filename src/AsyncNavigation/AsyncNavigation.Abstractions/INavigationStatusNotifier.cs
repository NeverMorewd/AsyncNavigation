using AsyncNavigation.Core;

namespace AsyncNavigation.Abstractions;

public interface INavigationStatusNotifier
{
    Task ViewActivatedAsync(NavigationContext context);
    Task ViewDeactivatedAsync(NavigationContext context);
}
