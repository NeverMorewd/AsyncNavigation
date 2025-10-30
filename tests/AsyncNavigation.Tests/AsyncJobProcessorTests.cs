using AsyncNavigation.Abstractions;
using AsyncNavigation.Core;
using AsyncNavigation.Tests.Infrastructure;
using AsyncNavigation.Tests.Mocks;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics;
using Xunit.Abstractions;

namespace AsyncNavigation.Tests;

public class AsyncJobProcessorTests : IClassFixture<ServiceFixture>
{
    private readonly ITestOutputHelper _testOutputHelper;
    private readonly IServiceProvider _serviceProvider;
    public AsyncJobProcessorTests(ServiceFixture serviceFixture, ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
        _serviceProvider = serviceFixture.ServiceProvider;
    }
    [Fact]
    public async Task RunJobAsync_Should_RunJobSuccessfully()
    {
        var processor = _serviceProvider.GetRequiredService<IAsyncJobProcessor>();
        using var context = new TestJobContext();
        var executed = false;

        await processor.RunJobAsync(context, async ctx =>
        {
            await Task.Delay(10);
            executed = true;
        }, NavigationJobStrategy.Queue);

        Assert.True(executed);
        Assert.True(context.Started);
        Assert.True(context.Completed);
    }

    [Fact]
    public async Task RunJobAsync_Should_Throw_When_SameJobIdStartedTwice()
    {
        var processor = _serviceProvider.GetRequiredService<IAsyncJobProcessor>();
        using var context = new TestJobContext();

        var task1 = processor.RunJobAsync(context, async ctx =>
        {
            await Task.Delay(50);
        }, NavigationJobStrategy.CancelCurrent);

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            processor.RunJobAsync(context,
            ctx => Task.CompletedTask,
            NavigationJobStrategy.CancelCurrent));

        await task1;
    }

    [Fact]
    public async Task CancelAllAsync_Should_CancelRunningJob()
    {
        var processor = _serviceProvider.GetRequiredService<IAsyncJobProcessor>();
        using var context = new TestJobContext();
        var canceled = false;

        var task = processor.RunJobAsync(context, async ctx =>
        {
            try
            {
                await Task.Delay(1000, ctx.CancellationToken);
            }
            catch (OperationCanceledException)
            {
                canceled = true;
                throw;
            }
        }, NavigationJobStrategy.CancelCurrent);

        await Task.Delay(50);

        await processor.CancelAllAsync();

        await Assert.ThrowsAsync<TaskCanceledException>(() => task);
        Assert.True(canceled);
    }

    [Fact]
    public async Task QueueStrategy_Should_ExecuteJobsInSequence()
    {
        var processor = _serviceProvider.GetRequiredService<IAsyncJobProcessor>();
        var context1 = new TestJobContext();
        var context2 = new TestJobContext();

        var order = new List<int>();

        var task1 = processor.RunJobAsync(context1, async _ =>
        {
            order.Add(1);
            await Task.Delay(100);
            order.Add(2);
        }, NavigationJobStrategy.Queue);

        var task2 = processor.RunJobAsync(context2, _ =>
        {
            order.Add(3);
            return Task.CompletedTask;
        }, NavigationJobStrategy.Queue);

        await Task.WhenAll(task1, task2);

        Assert.Equal(new[] { 1, 2, 3 }, order);
        Assert.True(context1.Completed);
        Assert.True(context2.Completed);
    }

    [Fact]
    public async Task RunJobAsync_Should_CallOnCompleted_WhenJobThrows()
    {
        var processor = _serviceProvider.GetRequiredService<IAsyncJobProcessor>();
        using var context = new TestJobContext();

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            processor.RunJobAsync(context, ctx =>
            {
                throw new InvalidOperationException("Simulated failure");
            }, NavigationJobStrategy.Queue));

        Assert.Equal("Simulated failure", ex.Message);
        Assert.True(context.Completed); 
    }

    [Fact]
    public async Task CancelAllAsync_Should_AllowNewJobsAfterCancellation()
    {
        var processor = _serviceProvider.GetRequiredService<IAsyncJobProcessor>();
        using var context1 = new TestJobContext();
        var canceled = false;

        var task1 = processor.RunJobAsync(context1, async ctx =>
        {
            try
            {
                await Task.Delay(1000, ctx.CancellationToken);
            }
            catch (OperationCanceledException)
            {
                canceled = true;
                throw;
            }
        }, NavigationJobStrategy.Queue);

        await Task.Delay(10);
        await processor.CancelAllAsync();

        await Assert.ThrowsAsync<TaskCanceledException>(() => task1);
        Assert.True(canceled);

        using var context2 = new TestJobContext();
        var task2 = processor.RunJobAsync(context2, _ => Task.CompletedTask, NavigationJobStrategy.Queue);

        await task2; 
        Assert.True(context2.Completed);
    }

    [Fact]
    public async Task QueueStrategy_Should_Handle_ConcurrentSubmissions()
    {
        var processor = _serviceProvider.GetRequiredService<IAsyncJobProcessor>();
        var contexts = Enumerable.Range(0, 5).Select(_ => new TestJobContext()).ToList();
        var tasks = new List<Task>();

        foreach (var ctx in contexts)
        {
            tasks.Add(Task.Run(() => processor.RunJobAsync(ctx, async _ =>
            {
                await Task.Delay(10);
            }, NavigationJobStrategy.Queue)));
        }

        await Task.WhenAll(tasks);

        foreach (var ctx in contexts)
        {
            Assert.True(ctx.Completed);
            ctx.Dispose();
        }
    }

    [Fact]
    [Trait("Category", "Stress")]
    public async Task RunJobAsync_QueueStrategy_Should_Handle_10000_ConcurrentJobs()
    {
        var processor = _serviceProvider.GetRequiredService<IAsyncJobProcessor>();
        var jobCount = 10_000;
        var contexts = new TestJobContext[jobCount];
        var tasks = new Task[jobCount];
        var stopwatch = Stopwatch.StartNew();

        _testOutputHelper.WriteLine($"Starting {jobCount} concurrent jobs with Queue strategy...");

        Parallel.For(0, jobCount, i =>
        {
            var context = new TestJobContext();
            contexts[i] = context;

            var task = processor.RunJobAsync(context, async ctx =>
            {
                await Task.Delay(1);
            }, NavigationJobStrategy.Queue);

            Volatile.Write(ref tasks[i], task);
        });

        await Task.WhenAll(tasks);

        stopwatch.Stop();

        var completedCount = contexts.Count(c => c.Completed);
        var startedCount = contexts.Count(c => c.Started);

        _testOutputHelper.WriteLine($"Completed: {completedCount}/{jobCount}");
        _testOutputHelper.WriteLine($"Started: {startedCount}/{jobCount}");
        _testOutputHelper.WriteLine($"Total Time: {stopwatch.ElapsedMilliseconds} ms");
        _testOutputHelper.WriteLine($"Avg Time per Job: {stopwatch.Elapsed.TotalMilliseconds / jobCount:F2} ms");

        Assert.Equal(jobCount, completedCount);
        Assert.Equal(jobCount, startedCount);

        foreach (var ctx in contexts)
            ctx?.Dispose();
    }

    [Fact]
    [Trait("Category", "Stress")]
    public async Task RunJobAsync_CancelCurrentStrategy_Should_Handle_RapidSuccessiveJobs()
    {
        var processor = _serviceProvider.GetRequiredService<IAsyncJobProcessor>();
        var iterations = 200;
        var cancellationDelayMs = 10;
        var jobDurationMs = 100;
        var stopwatch = Stopwatch.StartNew();
        List<TestJobContext> jobs = [];

        _testOutputHelper.WriteLine($"Starting {iterations} rapid jobs with CancelCurrent strategy...");

        for (int i = 0; i < iterations; i++)
        {
            var context = new TestJobContext();
            jobs.Add(context);
            var task = processor.RunJobAsync(context, async ctx =>
            {
                try
                {
                    await Task.Delay(jobDurationMs, ctx.CancellationToken);
                }
                catch (OperationCanceledException)
                {
                    
                }
            }, NavigationJobStrategy.CancelCurrent);

            await Task.Delay(cancellationDelayMs);
        }

        await processor.WaitAllAsync();
        stopwatch.Stop();

        _testOutputHelper.WriteLine($"Rapid cancellation test completed in {stopwatch.ElapsedMilliseconds} ms");
        _testOutputHelper.WriteLine($"Jobs in queue: {processor.JobsCount}");
        _testOutputHelper.WriteLine($"Jobs cancelled: {jobs.Where(j => j.CancellationToken.IsCancellationRequested).Count()}");
        _testOutputHelper.WriteLine($"Jobs completed: {jobs.Where(j => !j.CancellationToken.IsCancellationRequested).Count()}");

        Assert.Single(jobs.Where(j => !j.CancellationToken.IsCancellationRequested));

    }
}



