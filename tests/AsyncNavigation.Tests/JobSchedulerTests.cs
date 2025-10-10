using AsyncNavigation.Abstractions;
using AsyncNavigation.Core;

namespace AsyncNavigation.Tests;

public class JobSchedulerTests
{
    private class TestJobContext : IJobContext
    {
        public Guid JobId { get; } = Guid.NewGuid();
        public CancellationToken CancellationToken { get; set; } = CancellationToken.None;

        public bool Started { get; private set; }
        public bool Completed { get; private set; }

        public void OnStarted() => Started = true;
        public void OnCompleted() => Completed = true;

        public void LinkCancellationToken(CancellationToken otherToken)
        {
            return;
        }
    }

    [Fact]
    public async Task RunJobAsync_Should_RunJobSuccessfully()
    {
        var scheduler = new JobScheduler();
        var context = new TestJobContext();
        var executed = false;

        await scheduler.RunJobAsync(context, async ctx =>
        {
            await Task.Delay(10); 
            executed = true;
        });

        Assert.True(executed);
        Assert.True(context.Started);
        Assert.True(context.Completed);
    }

    [Fact]
    public async Task RunJobAsync_Should_Throw_When_SameJobIdStartedTwice()
    {
        var scheduler = new JobScheduler();
        var context = new TestJobContext();

        var task1 = scheduler.RunJobAsync(context, async ctx =>
        {
            await Task.Delay(50);
        });

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            scheduler.RunJobAsync(context, async ctx => { }));

        await task1;
    }

    [Fact]
    public async Task CancelAllAsync_Should_CancelRunningJob()
    {
        NavigationOptions.Default.NavigationJobStrategy = NavigationJobStrategy.CancelCurrent;

        var scheduler = new JobScheduler();
        var cts = new CancellationTokenSource();
        var context = new NavigationContext { RegionName = "", ViewName = "" };
        context.LinkCancellationToken(cts.Token);
        var canceled = false;

        var task = scheduler.RunJobAsync(context, async ctx =>
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
        });

        
        await Task.Delay(50);

        await scheduler.CancelAllAsync();

        await Assert.ThrowsAsync<TaskCanceledException>(() => task);
        Assert.True(canceled);
    }

    [Fact]
    public async Task QueueStrategy_Should_WaitForPreviousJob()
    {
        NavigationOptions.Default.NavigationJobStrategy = NavigationJobStrategy.Queue;

        var scheduler = new JobScheduler();
        var firstContext = new TestJobContext();
        var secondContext = new TestJobContext();
        var firstCompleted = false;
        var secondCompleted = false;

        var firstTask = scheduler.RunJobAsync(firstContext, async ctx =>
        {
            await Task.Delay(50);
            firstCompleted = true;
        });

        var secondTask = scheduler.RunJobAsync(secondContext, async ctx =>
        {
            secondCompleted = true;
        });

        await Task.WhenAll(firstTask, secondTask);

        Assert.True(firstCompleted);
        Assert.True(secondCompleted);
    }
}
