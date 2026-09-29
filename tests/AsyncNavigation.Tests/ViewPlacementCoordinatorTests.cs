namespace AsyncNavigation.Tests;

public sealed class ViewPlacementCoordinatorTests
{
    [Fact]
    public async Task EnterAsync_ReentrantFastPath_DoesNotDeadlock()
    {
        var coordinator = new ViewPlacementCoordinator();

        using var flow = coordinator.EnsureFlow();
        using var outer = await coordinator.EnterAsync("region");

        Assert.True(coordinator.IsHeld("region"));

        var inner = await coordinator.EnterAsync("region").WaitAsync(TimeSpan.FromSeconds(2));
        inner.Dispose();

        Assert.True(coordinator.IsHeld("region"));
    }

    [Fact]
    public async Task EnterAsync_AfterContendedAcquire_IsHeldAndReentrancyBothWork()
    {
        var coordinator = new ViewPlacementCoordinator();

        // Flow A takes the lease on the fast (uncontended) path and holds it open.
        using var flowA = coordinator.EnsureFlow();
        var leaseA = await coordinator.EnterAsync("region");

        // Flow B's EnterAsync must genuinely wait - it cannot complete until Flow A releases -
        // exercising the async EnterSlowAsync path rather than the synchronous fast path.
        var enterBTask = Task.Run(async () =>
        {
            using var flow = coordinator.EnsureFlow();
            var lease = await coordinator.EnterAsync("region");

            // Regression check: after acquiring via the contended/slow path, this same logical
            // flow must be recognized as the holder (previously IsHeld() spuriously read false
            // here because the depth marker never escaped the async helper that set it).
            Assert.True(coordinator.IsHeld("region"));

            // Regression check: a nested EnterAsync call for the same region, from the same flow
            // that just acquired it via the slow path, must be reentrant - not try to
            // re-acquire the same semaphore and deadlock against itself.
            var nested = await coordinator.EnterAsync("region").WaitAsync(TimeSpan.FromSeconds(2));
            nested.Dispose();

            lease.Dispose();
        });

        // Give Flow B's EnterAsync a moment to actually start waiting on the semaphore before
        // releasing Flow A, so it exercises the contended path rather than racing to be first.
        await Task.Delay(100);
        leaseA.Dispose();

        await enterBTask.WaitAsync(TimeSpan.FromSeconds(5));
    }
}
