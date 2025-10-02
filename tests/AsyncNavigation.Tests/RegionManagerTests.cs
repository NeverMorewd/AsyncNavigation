using AsyncNavigation.Abstractions;
using AsyncNavigation.Avalonia;
using AsyncNavigation.Tests.Mocks;
using AsyncNavigation.Tests.Utils;
using Microsoft.Extensions.DependencyInjection;

namespace AsyncNavigation.Tests;

public class RegionManagerTests : IClassFixture<ServiceFixture>
{
    private readonly RegionManager _regionManager;
    private readonly IServiceProvider _serviceProvider;

    public RegionManagerTests(ServiceFixture serviceFixture)
    {
        _serviceProvider = serviceFixture.ServiceProvider;
        _regionManager = new RegionManager(
            _serviceProvider.GetRequiredService<IRegionFactory>(),
            _serviceProvider);
    }

    [Fact]
    public void AddRegion_ShouldAddSuccessfully()
    {
        var region = new FakeRegion();
        _regionManager.AddRegion("Main", region);

        Assert.Contains("Main", _regionManager.Regions.Keys);
        Assert.Same(region, _regionManager.Regions["Main"]);
    }

    [Fact]
    public void AddRegion_ShouldThrow_WhenDuplicate()
    {
        var region = new FakeRegion();
        _regionManager.AddRegion("Main", region);

        Assert.Throws<InvalidOperationException>(() =>
            _regionManager.AddRegion("Main", new FakeRegion()));
    }

    [Fact]
    public async Task RequestNavigateAsync_ShouldActivateView()
    {
        var region = new FakeRegion();
        _regionManager.AddRegion("Main", region);
        var result = await _regionManager.RequestNavigateAsync("Main", "Home");
        Assert.True(result.IsSuccessful);
    }

    [Fact]
    public async Task RequestNavigateAsync_ShouldThrow_WhenRegionNotFound()
    {
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _regionManager.RequestNavigateAsync("Unknown", "Home"));
    }

    [Fact]
    public async Task TryGetRegion_ShouldReturnFalse_WhenRegionCollected()
    {
        var region = new FakeRegion();
        _regionManager.AddRegion("Temp", region);

        var weak = new WeakReference(region);
        region = null;
        var collected = await GcUtils.WaitForCollectedAsync(weak);
        if (!collected)
        {
            Assert.Fail("Region was not collected in time.");
        }

        var success = _regionManager.TryGetRegion("Temp", out var recovered);
        Assert.False(success);
        Assert.Null(recovered);

        
        Assert.DoesNotContain("Temp", _regionManager.Regions.Keys);
        Assert.False(weak.IsAlive);
    }

    [Fact]
    public void TryRemoveRegion_ShouldRemoveSuccessfully()
    {
        var region = new FakeRegion();
        _regionManager.AddRegion("Removable", region);

        var removed = _regionManager.TryRemoveRegion("Removable", out var removedRegion);
        Assert.True(removed);
        Assert.Same(region, removedRegion);

        
        var exists = _regionManager.TryGetRegion("Removable", out _);
        Assert.False(exists);
    }

    [Fact]
    public async Task Regions_ShouldNotContainCollectedRegion()
    {
        var region = new FakeRegion();
        _regionManager.AddRegion("GCRegion", region);

        var weak = new WeakReference(region);

        region = null;
        var collected = await GcUtils.WaitForCollectedAsync(weak);
        if (!collected)
        {
            Assert.Fail("Region was not collected in time.");
        }

        var regions = _regionManager.Regions;
        Assert.DoesNotContain("GCRegion", regions.Keys);
        Assert.False(weak.IsAlive);
    }
}
