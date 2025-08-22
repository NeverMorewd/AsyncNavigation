using AsyncNavigation.Core;

namespace AsyncNavigation.Abstractions;

public interface INavigationJobScheduler
{
    Task RunJobAsync(NavigationContext context, Func<NavigationContext, Task> navigationAction);
    Task CancelAllAsync();
    Task WaitAllAsync();
}
