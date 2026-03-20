using AsyncNavigation.Abstractions;
using AsyncNavigation.Tests.Infrastructure;
using AsyncNavigation.Tests.Mocks;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace AsyncNavigation.Tests;

[Collection("RegionManagerCollection")]
public class NavigationInterceptorTests
{
    private const string RegionName = "InterceptorRegion";

    private readonly IRegionManager _regionManager;
    private readonly IServiceProvider _serviceProvider;
    private readonly TrackingInterceptor _interceptor;

    public NavigationInterceptorTests(ServiceFixture fixture)
    {
        _serviceProvider = fixture.ServiceProvider;
        _regionManager = _serviceProvider.GetRequiredService<IRegionManager>();
        _interceptor = fixture.Interceptor;
        _interceptor.Reset();
    }

    private TestRegion BuildRegion()
    {
        var region = TestRegion.Build(_serviceProvider);
        _regionManager.TryRemoveRegion(RegionName, out _);
        _regionManager.AddRegion(RegionName, region);
        return region;
    }

    [Fact]
    public async Task OnNavigatingAsync_IsCalledBeforeNavigation()
    {
        BuildRegion();

        await _regionManager.RequestNavigateAsync(RegionName, "TestView");

        Assert.Single(_interceptor.NavigatingContexts);
        Assert.Equal("TestView", _interceptor.NavigatingContexts[0].ViewName);
    }

    [Fact]
    public async Task OnNavigatedAsync_IsCalledAfterSuccessfulNavigation()
    {
        BuildRegion();

        var result = await _regionManager.RequestNavigateAsync(RegionName, "TestView");

        Assert.True(result.IsSuccessful);
        Assert.Single(_interceptor.NavigatedContexts);
        Assert.Equal("TestView", _interceptor.NavigatedContexts[0].ViewName);
    }

    [Fact]
    public async Task OnNavigatingAsync_ThrowingOperationCanceled_CancelsNavigation()
    {
        _interceptor.ThrowOnNavigating = true;
        BuildRegion();

        var result = await _regionManager.RequestNavigateAsync(RegionName, "TestView");

        Assert.True(result.IsCancelled);
        Assert.Empty(_interceptor.NavigatedContexts);
    }

    [Fact]
    public async Task OnNavigatedAsync_ThrowingException_DoesNotAffectNavigationResult()
    {
        _interceptor.ThrowOnNavigated = true;
        BuildRegion();

        var result = await _regionManager.RequestNavigateAsync(RegionName, "TestView");

        // Result should still be successful; interceptor errors are swallowed
        Assert.True(result.IsSuccessful);
    }

    [Fact]
    public async Task OnNavigatedAsync_IsNotCalledOnFailedNavigation()
    {
        _interceptor.ThrowOnNavigating = true;
        BuildRegion();

        await _regionManager.RequestNavigateAsync(RegionName, "TestView");

        Assert.Empty(_interceptor.NavigatedContexts);
    }
}
