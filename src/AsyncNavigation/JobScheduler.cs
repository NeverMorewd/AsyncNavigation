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
        await HandleExistingJob();

        var cts = new CancellationTokenSource();
        jobContext.LinkCancellationToken(cts.Token);
        var task = jobAction(jobContext);

        if (!_jobs.TryAdd(jobContext.JobId, (task, cts)))
        {
            cts.Dispose();
            throw new InvalidOperationException($"Job with id {jobContext.JobId} is already started.");
        }

        try
        {
            jobContext.OnStarted();
            await task;
        }
        finally
        {
            jobContext.OnCompleted();
            if (_jobs.TryRemove(jobContext.JobId, out var jobToAbandon))
                jobToAbandon.Cts.Dispose();
        }
    }

    public Task WaitAllAsync() => Task.WhenAll(_jobs.Values.Select(j => j.Task));

    public Task CancelAllAsync()
    { 
        return Task.WhenAll(_jobs.Values.Select(job =>
        { 
            return job.Cts.CancelAsync();
        })); 
    }
    public Task CancelAllAsyncOld()
    {
        foreach (var cts in _jobs.Values.Select(j => j.Cts))
        {
            cts.Cancel();
        }
        return Task.CompletedTask;
    }
    private async ValueTask HandleExistingJob()
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
