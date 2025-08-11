using AsyncNavigation.Core;

namespace AsyncNavigation.Abstractions;

public interface INavigationTaskManager
{
    Task<NavigationResult> ExecuteNavigationAsync(NavigationContext context, Func<NavigationContext, Task> navigationAction);
    Task CancelAllTasksAsync();
    Task WaitAllTasksAsync();
}
