using AsyncNavigation.Abstractions;
using AsyncNavigation.Core;
using System.Collections.Concurrent;

namespace AsyncNavigation;

internal sealed class AsyncJobProcessor : IAsyncJobProcessor
{
    private readonly ConcurrentDictionary<Guid, (Lazy<Task> LazyTask, CancellationTokenSource Cts)> _jobs = new();

    int IAsyncJobProcessor.JobsCount => _jobs.Count;

    public async Task RunJobAsync<TContext>(
        TContext jobContext,
        Func<TContext, Task> jobAction,
        NavigationJobStrategy navigationJobStrategy) where TContext : IJobContext
    {
        await HandleExistingJobs(navigationJobStrategy);

        var cts = new CancellationTokenSource();
        jobContext.LinkCancellationToken(cts.Token);
        var lazyTask = new Lazy<Task>(() => jobAction(jobContext));
        if (!_jobs.TryAdd(jobContext.JobId, (lazyTask, cts)))
        {
            cts.Dispose();
            throw new InvalidOperationException($"Job with id {jobContext.JobId} is already started.");
        }

        jobContext.OnStarted();

        try
        {
            await lazyTask.Value;
        }
        finally
        {
            jobContext.OnCompleted();
            if (_jobs.TryRemove(jobContext.JobId, out var jobToAbandon))
                jobToAbandon.Cts.Dispose();
        }
    }

    public Task WaitAllAsync() => Task.WhenAll(_jobs.Values.Select(j => j.LazyTask.Value));

    public Task CancelAllAsync()
    { 
        return Task.WhenAll(_jobs.Values.Select(job =>
        { 
            return job.Cts.CancelAsync();
        })); 
    }
    private async ValueTask HandleExistingJobs(NavigationJobStrategy navigationJobStrategy)
    {
        if (_jobs.IsEmpty)
            return;

        switch (navigationJobStrategy)
        {
            case NavigationJobStrategy.CancelCurrent:
                // Cancel all in-flight jobs, then wait for them to actually finish.
                // CancelAsync() only signals cancellation; without WaitAllAsync() the
                // new job could start while previous jobs are still running their
                // finally/cleanup blocks, leading to concurrent navigation state.
                await CancelAllAsync();
                await WaitAllAsync();
                break;
            case NavigationJobStrategy.Queue:
                await WaitAllAsync();
                break;
        }
    }
}
