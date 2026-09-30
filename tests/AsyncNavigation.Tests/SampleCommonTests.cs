using AsyncNavigation.Abstractions;
using Moq;
using Sample.Common;

namespace AsyncNavigation.Tests;

public sealed class SampleCommonTests
{
    [Fact]
    public async Task SampleNavigationInterceptor_DoesNotThrow()
    {
        var interceptor = new SampleNavigationInterceptor();
        var context = new NavigationContext { RegionName = "MainRegion", ViewName = "LightView" };

        await interceptor.OnNavigatingAsync(context);
        await interceptor.OnNavigatedAsync(context);
    }

    [Fact]
    public async Task LightViewModel_CanNavigateAsync_AllowsLeavingByDefault()
    {
        var viewModel = new LightViewModel(Mock.Of<IRegionManager>());
        var context = new NavigationContext { RegionName = "MainRegion", ViewName = "HeavyView" };

        Assert.True(await viewModel.CanNavigateAsync(context, CancellationToken.None));
    }

    [Fact]
    public async Task LightViewModel_CanNavigateAsync_BlocksLeavingWhenPreventLeaveIsSet()
    {
        var viewModel = new LightViewModel(Mock.Of<IRegionManager>())
        {
            PreventLeave = true
        };
        var context = new NavigationContext { RegionName = "MainRegion", ViewName = "HeavyView" };

        Assert.False(await viewModel.CanNavigateAsync(context, CancellationToken.None));
    }
}
