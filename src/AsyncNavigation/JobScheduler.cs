using AsyncNavigation.Abstractions;
using AsyncNavigation.Core;
using System.Collections.Concurrent;

namespace AsyncNavigation;

internal sealed class JobScheduler : IJobScheduler
{
    private readonly ConcurrentDictionary<Guid, (Task Task, CancellationTokenSource Cts)> _jobs = new();

    public async Task RunJobAsync<TContext>(
        TContext jobContext,
        Func<TContext, Task> jobAction) where TContext : IJobContext
    {
        if (_jobs.ContainsKey(jobContext.JobId))
            throw new InvalidOperationException($"Job with id {jobContext.JobId} is already started.");

        await HandleExistingJob();

        var job = _jobs.GetOrAdd(jobContext.JobId, _ =>
        {
            var ctsForManualCancel = new CancellationTokenSource();
            jobContext.LinkCancellationToken(ctsForManualCancel.Token);
            var task = jobAction(jobContext);
            return (task, ctsForManualCancel);
        });

        try
        {
            jobContext.OnStarted();
            await job.Task;
        }
        finally
        {
            jobContext.OnCompleted();
            if (_jobs.TryRemove(jobContext.JobId, out var jobToAbandon))
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
