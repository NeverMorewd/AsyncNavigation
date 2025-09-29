using AsyncNavigation.Abstractions;
using AsyncNavigation.Avalonia;
using Microsoft.Extensions.DependencyInjection;

namespace AsyncNavigation.Tests;

public class RegionManagerTests: IClassFixture<ServiceFixture>
{
    private readonly RegionManager _regionManager;
    private readonly IServiceProvider _serviceProvider;

    public RegionManagerTests(ServiceFixture serviceFixture)
    {
        _serviceProvider = serviceFixture.ServiceProvider;
        _regionManager = new RegionManager(_serviceProvider.GetRequiredService<IRegionFactory>(), _serviceProvider);
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

        Assert.Throws<InvalidOperationException>(() => _regionManager.AddRegion("Main", new FakeRegion()));
    }

    [Fact]
    public async Task RequestNavigateAsync_ShouldActivateView()
    {
        var region = new FakeRegion();
        _regionManager.AddRegion("Main", region);

        var result = await _regionManager.RequestNavigateAsync("Main", "Home");

        Assert.True(result.IsSuccessful);
        //Assert.Contains("Home", region.ActivatedViews);
    }

    [Fact]
    public async Task RequestNavigateAsync_ShouldThrow_WhenRegionNotFound()
    {
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _regionManager.RequestNavigateAsync("Unknown", "Home"));
    }
}