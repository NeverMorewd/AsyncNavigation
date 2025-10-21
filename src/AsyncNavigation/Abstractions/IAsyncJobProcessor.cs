using AsyncNavigation.Core;

namespace AsyncNavigation.Abstractions;

internal interface IAsyncJobProcessor
{
    int JobsCount { get; }
    Task RunJobAsync<TContext>(TContext context, 
        Func<TContext, Task> navigationAction,
        NavigationJobStrategy navigationJobStrategy) where TContext : IJobContext;
    Task CancelAllAsync();
    Task WaitAllAsync();
}
