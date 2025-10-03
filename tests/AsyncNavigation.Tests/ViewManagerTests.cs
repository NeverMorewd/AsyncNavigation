using AsyncNavigation.Abstractions;
using AsyncNavigation.Tests.Infrastructure;
using AsyncNavigation.Tests.Utils;
using Microsoft.Extensions.DependencyInjection;

namespace AsyncNavigation.Tests;

public class ViewManagerTests : IClassFixture<ServiceFixture>
{
    private readonly IServiceProvider _serviceProvider;
    public ViewManagerTests(ServiceFixture fixture)
    {
        _serviceProvider = fixture.ServiceProvider;
    }

    [Fact]
    public async Task Remove_Dispose_View_Should_BeCollected()
    {
        var manager = _serviceProvider.GetRequiredService<IViewManager>();
        var view = await manager.ResolveViewAsync("TestView", true);
        manager.Remove("TestView", dispose: true);
        view = null!;
        await GcUtils.AssertCollectedAsync(view);
    }

    [Fact]
    public async Task Exceed_MaxCache_Should_Evict_Oldest()
    {
        NavigationOptions.Default.MaxCachedViews = 1;
        var manager = _serviceProvider.GetRequiredService<IViewManager>();
        var v1 = await manager.ResolveViewAsync("TestView", true);
        var v2 = await manager.ResolveViewAsync("AnotherTestView", true);
        v1 = null!;
        await GcUtils.AssertCollectedAsync(v1);
        Assert.NotNull(v2);
    }

    [Fact]
    public async Task Clear_Should_Release_All()
    {
        var manager = _serviceProvider.GetRequiredService<IViewManager>();

        var v1 = await manager.ResolveViewAsync("TestView", true);
        var v2 = await manager.ResolveViewAsync("AnotherTestView", true);

        manager.Clear();
        v1 = null!;
        v2 = null!;
        await GcUtils.AssertCollectedAsync(v1);
        await GcUtils.AssertCollectedAsync(v2);
    }
}
