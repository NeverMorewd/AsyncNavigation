using AsyncNavigation.Abstractions;
using AsyncNavigation.Tests.Infrastructure;
using AsyncNavigation.Tests.Mocks;
using AsyncNavigation.Tests.Utils;
using Microsoft.Extensions.DependencyInjection;

namespace AsyncNavigation.Tests;

[Collection("RegionManagerCollection")]
public class RegionManagerTests
{
    private readonly IRegionManager _regionManager;
    private readonly IServiceProvider _serviceProvider;

    public RegionManagerTests(ServiceFixture serviceFixture)
    {
        _serviceProvider = serviceFixture.ServiceProvider;
        _regionManager = _serviceProvider.GetRequiredService<IRegionManager>();
    }

    [Fact]
    public void AddRegion_ShouldAddSuccessfully()
    {
        var region = TestRegion.Build(_serviceProvider);
        _regionManager.TryRemoveRegion("Main", out _);
        _regionManager.AddRegion("Main", region);

        Assert.Contains("Main", _regionManager.Regions.Keys);
        Assert.Same(region, _regionManager.Regions["Main"]);
    }

    [Fact]
    public void AddRegion_ShouldThrow_WhenDuplicate()
    {
        var region = TestRegion.Build(_serviceProvider);
        _regionManager.TryRemoveRegion("Main", out _);
        _regionManager.AddRegion("Main", region);

        Assert.Throws<InvalidOperationException>(() =>
            _regionManager.AddRegion("Main", TestRegion.Build(_serviceProvider)));
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
        var region = TestRegion.Build(_serviceProvider);
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
        var region = TestRegion.Build(_serviceProvider);
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
        var region = TestRegion.Build(_serviceProvider);
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

    [Fact]
    public async Task RequestNavigateAsync_ShouldActivateView()
    {
        var region = TestRegion.Build(_serviceProvider);
        _regionManager.TryRemoveRegion("Main", out _);
        _regionManager.AddRegion("Main", region);
        var result = await _regionManager.RequestNavigateAsync("Main", "TestView");
        Assert.True(result.IsSuccessful);
    }

    [Fact]
    public async Task RequestNavigateAsync_ShouldCancel()
    {
        var region = TestRegion.Build(_serviceProvider);
        _regionManager.TryRemoveRegion("Main", out _);
        _regionManager.AddRegion("Main", region);
        CancellationTokenSource cancellationTokenSource = new();
        cancellationTokenSource.Cancel();
        var result = await _regionManager.RequestNavigateAsync("Main",
            "TestView",
            cancellationToken: cancellationTokenSource.Token);
        Assert.True(result.IsCancelled);
    }
    [Fact]
    public async Task GoBack_ShouldActivateView()
    {
        var region = TestRegion.Build(_serviceProvider);
        _regionManager.TryRemoveRegion("Main", out _);
        _regionManager.AddRegion("Main", region);
        _ = await _regionManager.RequestNavigateAsync("Main", "TestView");
        _ = await _regionManager.RequestNavigateAsync("Main", "AnotherTestView");
        var result = await _regionManager.GoBackAsync("Main");
        Assert.True(result.IsSuccessful);
    }
    [Fact]
    public async Task GoBack_ShouldCancel()
    {
        var region = TestRegion.Build(_serviceProvider);
        _regionManager.TryRemoveRegion("Main", out _);
        _regionManager.AddRegion("Main", region);
        CancellationTokenSource cancellationTokenSource = new();
        cancellationTokenSource.Cancel();
        _ = await _regionManager.RequestNavigateAsync("Main", "TestView");
        _ = await _regionManager.RequestNavigateAsync("Main", "AnotherTestView");
        var result = await _regionManager.GoBackAsync("Main", cancellationToken: cancellationTokenSource.Token);
        Assert.True(result.IsCancelled);
    }

    [Fact]
    public async Task NavigationGuard_WhenAllowsNavigation_ShouldSucceed()
    {
        var region = TestRegion.Build(_serviceProvider);
        _regionManager.TryRemoveRegion("Guard1", out _);
        _regionManager.AddRegion("Guard1", region);

        _ = await _regionManager.RequestNavigateAsync("Guard1", "GuardTestView");
        var guardVm = GuardTestNavigationAware.LastCreated!;
        guardVm.AllowNavigation = true;

        var result = await _regionManager.RequestNavigateAsync("Guard1", "TestView");

        Assert.True(result.IsSuccessful);
    }

    [Fact]
    public async Task NavigationGuard_WhenBlocksNavigation_ShouldCancel()
    {
        var region = TestRegion.Build(_serviceProvider);
        _regionManager.TryRemoveRegion("Guard2", out _);
        _regionManager.AddRegion("Guard2", region);

        _ = await _regionManager.RequestNavigateAsync("Guard2", "GuardTestView");
        var guardVm = GuardTestNavigationAware.LastCreated!;
        guardVm.AllowNavigation = false;

        var result = await _regionManager.RequestNavigateAsync("Guard2", "TestView");

        // Cleanup: reset guard state and clear _currentRegion so subsequent tests are not affected
        guardVm.AllowNavigation = true;
        _regionManager.TryRemoveRegion("Guard2", out _);

        Assert.True(result.IsCancelled);
    }

    [Fact]
    public async Task NavigationGuard_WhenBlocksNavigation_DoesNotCallOnNavigatedFrom()
    {
        var region = TestRegion.Build(_serviceProvider);
        _regionManager.TryRemoveRegion("Guard3", out _);
        _regionManager.AddRegion("Guard3", region);

        _ = await _regionManager.RequestNavigateAsync("Guard3", "GuardTestView");
        var guardVm = GuardTestNavigationAware.LastCreated!;
        guardVm.AllowNavigation = false;

        _ = await _regionManager.RequestNavigateAsync("Guard3", "TestView");

        // Cleanup: reset guard state and clear _currentRegion so subsequent tests are not affected
        guardVm.AllowNavigation = true;
        _regionManager.TryRemoveRegion("Guard3", out _);

        Assert.True(guardVm.GuardWasCalled);
        Assert.False(guardVm.NavigatedFromWasCalled);
    }
}
