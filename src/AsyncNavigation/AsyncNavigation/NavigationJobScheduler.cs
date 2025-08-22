using AsyncNavigation.Abstractions;
using AsyncNavigation.Core;
using System.Collections.Concurrent;

namespace AsyncNavigation;

internal sealed class NavigationJobScheduler : INavigationJobScheduler
{
    private readonly ConcurrentDictionary<NavigationContext, (Task Task, CancellationTokenSource Cts)> _jobs = new();

    public async Task RunJobAsync(NavigationContext navigationContext, Func<NavigationContext, Task> navigationTaskAction)
    {
        if (_jobs.ContainsKey(navigationContext))
            throw new InvalidOperationException($"Navigation task of {navigationContext} is already started.");

        await HandleExistingJob();

        var job = _jobs.GetOrAdd(navigationContext, _ =>
        {
            var ctsForManualCancel = new CancellationTokenSource();
            var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(
                navigationContext.CancellationToken,
                ctsForManualCancel.Token);

            navigationContext.CancellationToken = linkedCts.Token;
            var task = navigationTaskAction(navigationContext);
            return (task, ctsForManualCancel);
        });

        using var register = navigationContext.CancellationToken.Register(() =>
        {
            navigationContext.WithStatus(NavigationStatus.Cancelled);
        });

        try
        {
            navigationContext.WithStatus(NavigationStatus.InProgress);
            await job.Task;
            navigationContext.WithStatus(NavigationStatus.Succeeded);
        }
        finally
        {
            if (_jobs.TryRemove(navigationContext, out var jobToAbandon))
                jobToAbandon.Cts.Dispose();
        }
    }

    public Task WaitAllAsync() => Task.WhenAll(_jobs.Values.Select(j => j.Task));

    public Task CancelAllAsync() => Task.WhenAll(_jobs.Values.Select(j => j.Cts.CancelAsync()));

    private async Task HandleExistingJob()
    {
        if (_jobs.IsEmpty)
            return;

        switch (NavigationOptions.Default.NavigationJobStrategy)
        {
            case NavigationJobStrategy.CancelCurrent:
                await CancelAllAsync();
                break;
            case NavigationJobStrategy.Queue:
                await WaitAllAsync();
                break;
        }
    }
}
