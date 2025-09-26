namespace AsyncNavigation.Abstractions;

internal interface INavigationJobScheduler
{
    Task RunJobAsync(NavigationContext context, Func<NavigationContext, Task> navigationAction);
    Task CancelAllAsync();
    Task WaitAllAsync();
}
