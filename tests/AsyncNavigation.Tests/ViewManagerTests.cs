using AsyncNavigation.Abstractions;
using AsyncNavigation.Core;
using AsyncNavigation.Tests.Utils;

namespace AsyncNavigation.Tests;

public class ViewManagerTests
{
    private sealed class TestView : IView
    {
        public object? DataContext { get; set; }
    }

    private sealed class TestViewFactory : IViewFactory
    {
        public void AddView(string key, IView view)
        {
            throw new NotImplementedException();
        }

        public void AddView(string key, Func<string, IView> viewBuilder)
        {
            throw new NotImplementedException();
        }

        public bool CanCreateView(string viewName)
        {
            throw new NotImplementedException();
        }

        public IView CreateView(string key) => new TestView { DataContext = new object() };
    }

    private static ViewManager CreateManager(int maxCache = 10)
    {
        var options = new NavigationOptions
        {
            ViewCacheStrategy = ViewCacheStrategy.IgnoreDuplicateKey,
            MaxCachedViews = maxCache
        };
        return new ViewManager(options, new TestViewFactory());
    }

    [Fact]
    public async Task Remove_Dispose_View_Should_BeCollected()
    {
        var manager = CreateManager();
        var view = await manager.ResolveViewAsync("A", true);
        manager.Remove("A", dispose: true);
        view = null!;
        await GcUtils.AssertCollectedAsync(view);
    }

    [Fact]
    public async Task Exceed_MaxCache_Should_Evict_Oldest()
    {
        var manager = CreateManager(maxCache: 1);
        var v1 = await manager.ResolveViewAsync("A", true);
        var v2 = await manager.ResolveViewAsync("B", true);
        v1 = null!;
        await GcUtils.AssertCollectedAsync(v1);
        Assert.NotNull(v2);
    }

    [Fact]
    public async Task Clear_Should_Release_All()
    {
        var manager = CreateManager();

        var v1 = await manager.ResolveViewAsync("A", true);
        var v2 = await manager.ResolveViewAsync("B", true);

        manager.Clear();
        v1 = null!;
        v2 = null!;
        await GcUtils.AssertCollectedAsync(v1);
        await GcUtils.AssertCollectedAsync(v2);
    }
}
