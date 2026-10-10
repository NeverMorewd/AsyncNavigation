using AsyncNavigation.Abstractions;
using AsyncNavigation.Floating;
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

    [Fact]
    public async Task LightViewModel_FloatThenDock_TogglesIsFloatingAndButtonText()
    {
        var floatingViews = new List<IFloatingViewSession>();
        var sessionMock = new Mock<IFloatingViewSession>();
        var context = new NavigationContext { RegionName = "MainRegion", ViewName = "LightView" };

        sessionMock.SetupGet(s => s.NavigationId).Returns(context.NavigationId);
        sessionMock.SetupGet(s => s.State).Returns(ViewPlacementState.Restored);
        sessionMock
            .Setup(s => s.RestoreAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask)
            .Callback(() =>
            {
                floatingViews.Remove(sessionMock.Object);
                sessionMock.Raise(s => s.StateChanged += null, EventArgs.Empty);
            });

        var placementService = new Mock<IViewPlacementService>();
        placementService.Setup(p => p.FloatingViews).Returns(() => floatingViews);
        placementService
            .Setup(p => p.FloatAsync(It.IsAny<string>(), It.IsAny<Guid?>(), It.IsAny<FloatingWindowOptions>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(() =>
            {
                floatingViews.Add(sessionMock.Object);
                return sessionMock.Object;
            });

        var viewModel = new LightViewModel(Mock.Of<IRegionManager>(), placementService.Object);
        await viewModel.OnNavigatedToAsync(context);

        Assert.Equal("Float", viewModel.FloatButtonText);

        await viewModel.FloatViewCommand.ExecuteAsync(null);
        Assert.True(viewModel.IsFloating);
        Assert.Equal("Dock to region", viewModel.FloatButtonText);

        await viewModel.FloatViewCommand.ExecuteAsync(null);
        Assert.False(viewModel.IsFloating);
        Assert.Equal("Float", viewModel.FloatButtonText);
    }
}
