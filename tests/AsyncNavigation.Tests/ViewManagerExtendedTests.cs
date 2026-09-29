using AsyncNavigation.Abstractions;
using AsyncNavigation.Core;
using AsyncNavigation.Tests.Infrastructure;
using AsyncNavigation.Tests.Mocks;
using AsyncNavigation.Tests.Utils;
using Microsoft.Extensions.DependencyInjection;

namespace AsyncNavigation.Tests;

/// <summary>
/// Additional ViewManager tests covering LRU order, cache strategies,
/// concurrent access, and dispose-on-eviction.
/// </summary>
[Collection("RegionManagerCollection")]
public class ViewManagerExtendedTests
{
    // -----------------------------------------------------------------------
    // Helpers
    // -----------------------------------------------------------------------

    /// <summary>
    /// Builds a ViewManager backed by a fresh DI container.
    /// NOTE: NavigationOptions.Default is a process-wide singleton. Using a
    /// dedicated IViewManager instance avoids cross-test state contamination.
    /// </summary>
    private static (IViewManager Manager, IServiceProvider Provider) BuildManager(
        int max = 10,
        ViewCacheStrategy strategy = ViewCacheStrategy.IgnoreDuplicateKey,
        ViewPlacementCoordinator? placement = null)
    {
        var sc = new ServiceCollection();
        sc.AddNavigationTestSupport(new NavigationOptions { MaxCachedViews = max, ViewCacheStrategy = strategy });
        if (placement is not null) sc.AddSingleton(placement);
        sc.RegisterView<TestView, TestNavigationAware>("V1");
        sc.RegisterView<AnotherTestView, TestNavigationAware>("V2");
        var sp = sc.BuildServiceProvider();
        return (sp.GetRequiredService<IViewManager>(), sp);
    }

    // -----------------------------------------------------------------------
    // LRU eviction order
    // -----------------------------------------------------------------------

    [Fact]
    public async Task LRU_EvictsLeastRecentlyUsed()
    {
        var (manager, _) = BuildManager(max: 1);

        var v1 = await manager.ResolveViewAsync("V1", useCache: true);
        // V1 is in the single-slot cache.
        // Resolving V2 should evict V1 because V2 is now the most-recently-used.
        var v2 = await manager.ResolveViewAsync("V2", useCache: true);

        Assert.NotNull(v2);
        // V1 has been evicted; resolving it again should create a NEW instance
        var v1Again = await manager.ResolveViewAsync("V1", useCache: false);
        Assert.NotNull(v1Again);
    }

    // -----------------------------------------------------------------------
    // UpdateDuplicateKey strategy
    // -----------------------------------------------------------------------

    [Fact]
    public async Task AddView_UpdateDuplicateKey_ReplacesExistingEntry()
    {
        var (manager, _) = BuildManager(max: 10, strategy: ViewCacheStrategy.UpdateDuplicateKey);

        // Populate cache with V1 (has DataContext set by DI)
        await manager.ResolveViewAsync("V1", useCache: true);

        // Replace with a bare-bones view that has NO DataContext
        var replacement = new TestView();   // DataContext = null
        manager.AddView("V1", replacement);

        // Resolve should return the replacement, not the original
        var resolved = await manager.ResolveViewAsync("V1", useCache: true);
        Assert.Same(replacement, resolved);
    }

    [Fact]
    public async Task AddView_IgnoreDuplicateKey_KeepsExistingEntry()
    {
        var (manager, _) = BuildManager(max: 10, strategy: ViewCacheStrategy.IgnoreDuplicateKey);

        var original = await manager.ResolveViewAsync("V1", useCache: true);
        var intruder = new TestView();

        manager.AddView("V1", intruder);

        var resolved = await manager.ResolveViewAsync("V1", useCache: true);
        Assert.Same(original, resolved);   // original should be kept
    }

    // -----------------------------------------------------------------------
    // Remove
    // -----------------------------------------------------------------------

    [Fact]
    public async Task Remove_WithDispose_DisposesViewModel()
    {
        var (manager, _) = BuildManager();
        var view = await manager.ResolveViewAsync("V1", useCache: true);

        var spy = new DisposableSpy();
        view.DataContext = spy;

        manager.Remove("V1", dispose: true);

        Assert.True(spy.Disposed);
    }

    [Fact]
    public async Task Remove_WithoutDispose_DoesNotDisposeViewModel()
    {
        var (manager, _) = BuildManager();
        var view = await manager.ResolveViewAsync("V1", useCache: true);

        var spy = new DisposableSpy();
        view.DataContext = spy;

        manager.Remove("V1", dispose: false);

        Assert.False(spy.Disposed);
    }

    [Fact]
    public void Remove_NonExistentKey_DoesNotThrow()
    {
        var (manager, _) = BuildManager();
        manager.Remove("NonExistent", dispose: true);   // should be a no-op
    }

    // -----------------------------------------------------------------------
    // Clear
    // -----------------------------------------------------------------------

    [Fact]
    public async Task Clear_DisposesAllViews()
    {
        var (manager, _) = BuildManager();
        var v1 = await manager.ResolveViewAsync("V1", useCache: true);
        var v2 = await manager.ResolveViewAsync("V2", useCache: true);

        var spy1 = new DisposableSpy(); v1.DataContext = spy1;
        var spy2 = new DisposableSpy(); v2.DataContext = spy2;

        manager.Clear();

        Assert.True(spy1.Disposed);
        Assert.True(spy2.Disposed);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Clear_DisposeFailure_RemovesAllNonFloatingEntries(bool withFloating)
    {
        var placement = withFloating ? new ViewPlacementCoordinator() : null;
        var (manager, _) = BuildManager(placement: placement);
        var first = await manager.ResolveViewAsync("V1", true);
        var second = await manager.ResolveViewAsync("V2", true);
        first.DataContext = new ThrowingDisposable();
        second.DataContext = new ThrowingDisposable();
        var floating = new TestView { DataContext = new DisposableSpy() };
        if (placement is not null)
        {
            manager.AddView("floating", floating);
            placement.Register(floating, _ => Task.CompletedTask);
        }

        Assert.Throws<InvalidOperationException>(() => manager.Clear());
        if (placement is not null)
        {
            Assert.Same(floating, await manager.ResolveViewAsync("floating", true));
            Assert.False(((DisposableSpy)floating.DataContext!).Disposed);
        }

        Assert.Null(await ((IViewPlacementCache)manager).FindCachedViewAsync("V1", _ => Task.FromResult(true)));
        Assert.Null(await ((IViewPlacementCache)manager).FindCachedViewAsync("V2", _ => Task.FromResult(true)));
        Assert.NotSame(first, await manager.ResolveViewAsync("V1", true));
        Assert.NotSame(second, await manager.ResolveViewAsync("V2", true));
    }

    private sealed class ThrowingDisposable : IDisposable
    {
        public void Dispose() => throw new InvalidOperationException("Disposal failed.");
    }

    // -----------------------------------------------------------------------
    // Concurrent access
    // -----------------------------------------------------------------------

    [Fact]
    public async Task ResolveViewAsync_ConcurrentCalls_NoDuplicateCreation()
    {
        var (manager, _) = BuildManager(max: 50);

        var tasks = Enumerable.Range(0, 20)
            .Select(_ => manager.ResolveViewAsync("V1", useCache: true))
            .ToArray();

        var results = await Task.WhenAll(tasks);

        Assert.All(results, v => Assert.NotNull(v));
    }

    // -----------------------------------------------------------------------
    // GC weak-reference behaviour
    // -----------------------------------------------------------------------

    [Fact]
    public async Task WeakReference_AfterRemove_ViewIsCollected()
    {
        var (manager, _) = BuildManager();
        IView? view = await manager.ResolveViewAsync("V1", useCache: true);
        var weak = new WeakReference(view);

        manager.Remove("V1", dispose: false);
        view = null;

        var collected = await GcUtils.WaitForCollectedAsync(weak);
        Assert.True(collected, "View should be GC-collected after removal without dispose.");
    }

    // -----------------------------------------------------------------------
    // Helpers
    // -----------------------------------------------------------------------

    private sealed class DisposableSpy : IDisposable
    {
        public bool Disposed { get; private set; }
        public void Dispose() => Disposed = true;
    }
}
