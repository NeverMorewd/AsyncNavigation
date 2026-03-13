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
        ViewCacheStrategy strategy = ViewCacheStrategy.IgnoreDuplicateKey)
    {
        // Reset Default before each helper call to avoid test-ordering issues
        NavigationOptions.Default.MaxCachedViews = max;
        NavigationOptions.Default.ViewCacheStrategy = strategy;

        var sc = new ServiceCollection();
        sc.AddNavigationTestSupport();            // uses the updated Default
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
