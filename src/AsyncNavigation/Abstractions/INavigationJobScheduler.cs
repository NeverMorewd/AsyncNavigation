namespace AsyncNavigation.Abstractions;

internal interface IJobScheduler
{
    Task RunJobAsync<TContext>(TContext context, Func<TContext, Task> navigationAction) where TContext : IJobContext;
    Task CancelAllAsync();
    Task WaitAllAsync();
}
