using AsyncNavigation.Core;

namespace AsyncNavigation.Abstractions;

public interface INavigationTaskManager
{
    Task StartNavigationAsync(NavigationContext context, Func<NavigationContext, Task> navigationAction);
    Task CancelAllAsync();
    Task WaitAllAsync();
}
