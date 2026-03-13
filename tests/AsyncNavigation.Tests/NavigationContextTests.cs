using AsyncNavigation.Core;

namespace AsyncNavigation.Tests;

/// <summary>
/// Tests for NavigationContext cancellation token chaining and status transitions.
/// These tests specifically cover the scenario raised in code-review:
/// LinkCancellationToken() called multiple times must keep the full chain alive.
/// </summary>
public class NavigationContextTests
{
    private static NavigationContext MakeContext() =>
        new() { RegionName = "Main", ViewName = "TestView" };

    // -----------------------------------------------------------------------
    // Initial state
    // -----------------------------------------------------------------------

    [Fact]
    public void Initial_CancellationToken_IsNone()
    {
        var ctx = MakeContext();
        Assert.Equal(CancellationToken.None, ctx.CancellationToken);
    }

    [Fact]
    public void Initial_Status_IsPending()
    {
        var ctx = MakeContext();
        Assert.Equal(NavigationStatus.Pending, ctx.Status);
        Assert.False(ctx.IsInProgress);
        Assert.False(ctx.IsCompleted);
    }

    // -----------------------------------------------------------------------
    // LinkCancellationToken – single link
    // -----------------------------------------------------------------------

    [Fact]
    public void LinkCancellationToken_NonCancelable_IsNoOp()
    {
        var ctx = MakeContext();
        // CancellationToken.None has CanBeCanceled == false; LinkCancellationToken should no-op
        ctx.LinkCancellationToken(CancellationToken.None);
        // Token should remain uncanceled
        Assert.False(ctx.CancellationToken.IsCancellationRequested);
    }

    [Fact]
    public void LinkCancellationToken_SingleLink_PropagatesCancellation()
    {
        var ctx = MakeContext();
        using var cts = new CancellationTokenSource();

        ctx.LinkCancellationToken(cts.Token);
        cts.Cancel();

        Assert.True(ctx.CancellationToken.IsCancellationRequested);
    }

    // -----------------------------------------------------------------------
    // LinkCancellationToken – multiple links (the GPT-raised scenario)
    // -----------------------------------------------------------------------

    [Fact]
    public void LinkCancellationToken_MultipleLinks_FirstTokenStillPropagates()
    {
        // Arrange – simulates the real flow:
        //   1. AsyncJobProcessor calls LinkCancellationToken with the job CTS
        //   2. NavigationContext links the external request token
        var ctx = MakeContext();
        using var jobCts = new CancellationTokenSource();       // 1st link (job token)
        using var requestCts = new CancellationTokenSource();   // 2nd link (request token)

        ctx.LinkCancellationToken(jobCts.Token);
        ctx.LinkCancellationToken(requestCts.Token);

        // Cancelling the FIRST (job) token must still cancel the context
        jobCts.Cancel();

        Assert.True(ctx.CancellationToken.IsCancellationRequested,
            "Cancelling the first-linked token must propagate through the full chain.");
    }

    [Fact]
    public void LinkCancellationToken_MultipleLinks_SecondTokenStillPropagates()
    {
        var ctx = MakeContext();
        using var jobCts = new CancellationTokenSource();
        using var requestCts = new CancellationTokenSource();

        ctx.LinkCancellationToken(jobCts.Token);
        ctx.LinkCancellationToken(requestCts.Token);

        // Cancelling the SECOND (request) token must also cancel the context
        requestCts.Cancel();

        Assert.True(ctx.CancellationToken.IsCancellationRequested,
            "Cancelling the second-linked token must propagate through the full chain.");
    }

    [Fact]
    public void LinkCancellationToken_MultipleLinks_NeitherCancelled_TokenIsLive()
    {
        var ctx = MakeContext();
        using var cts1 = new CancellationTokenSource();
        using var cts2 = new CancellationTokenSource();

        ctx.LinkCancellationToken(cts1.Token);
        ctx.LinkCancellationToken(cts2.Token);

        Assert.False(ctx.CancellationToken.IsCancellationRequested);
    }

    // -----------------------------------------------------------------------
    // CancelAndWaitAsync
    // -----------------------------------------------------------------------

    [Fact]
    public async Task CancelAndWaitAsync_CancelsToken_AndCompletesWhenNavigationEnds()
    {
        // We drive the context through the job processor so OnStarted / OnCompleted are called.
        var processor = new AsyncJobProcessor();
        var ctx = MakeContext();

        var navTask = processor.RunJobAsync(ctx, async c =>
        {
            await Task.Delay(Timeout.Infinite, c.CancellationToken);
        }, NavigationJobStrategy.Queue);

        // Give the job a moment to start
        await Task.Delay(30);

        var result = await ctx.CancelAndWaitAsync(timeout: TimeSpan.FromSeconds(5));

        Assert.True(result);
        Assert.True(ctx.CancellationToken.IsCancellationRequested);

        // Consume the expected cancellation exception
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => navTask);
    }

    [Fact]
    public async Task CancelAndWaitAsync_Timeout_ReturnsFalse()
    {
        // Never complete the job – timeout should return false
        var ctx = MakeContext();
        using var cts = new CancellationTokenSource();
        ctx.LinkCancellationToken(cts.Token);

        // Simulate "in-progress" by manually starting (no job processor needed for this path).
        // Use a long-lived CTS so navigation never completes.
        var result = await ctx.CancelAndWaitAsync(timeout: TimeSpan.FromMilliseconds(50));

        // The completionTcs was never set, so we expect false (timeout)
        Assert.False(result);
    }

    // -----------------------------------------------------------------------
    // Status transitions via AsyncJobProcessor
    // -----------------------------------------------------------------------

    [Fact]
    public async Task Status_InProgress_DuringJobExecution()
    {
        var processor = new AsyncJobProcessor();
        var ctx = MakeContext();
        NavigationStatus? statusDuringJob = null;

        await processor.RunJobAsync(ctx, c =>
        {
            statusDuringJob = ctx.Status;
            return Task.CompletedTask;
        }, NavigationJobStrategy.Queue);

        Assert.Equal(NavigationStatus.InProgress, statusDuringJob);
    }

    [Fact]
    public async Task Duration_SetAfterCompletion()
    {
        var processor = new AsyncJobProcessor();
        var ctx = MakeContext();

        await processor.RunJobAsync(ctx, _ => Task.CompletedTask, NavigationJobStrategy.Queue);

        Assert.NotNull(ctx.Duration);
        Assert.True(ctx.Duration!.Value >= TimeSpan.Zero);
    }

    [Fact]
    public void NavigationId_IsUnique()
    {
        var ids = Enumerable.Range(0, 100)
                            .Select(_ => MakeContext().NavigationId)
                            .ToHashSet();
        Assert.Equal(100, ids.Count);
    }

    // -----------------------------------------------------------------------
    // ToString
    // -----------------------------------------------------------------------

    [Fact]
    public void ToString_ContainsViewAndRegionName()
    {
        var ctx = MakeContext();
        var str = ctx.ToString();
        Assert.Contains("TestView", str);
        Assert.Contains("Main", str);
    }
}
