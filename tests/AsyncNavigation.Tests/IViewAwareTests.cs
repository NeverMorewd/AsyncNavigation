using AsyncNavigation.Abstractions;
using AsyncNavigation.Tests.Infrastructure;
using AsyncNavigation.Tests.Mocks;
using Microsoft.Extensions.DependencyInjection;

namespace AsyncNavigation.Tests;

/// <summary>
/// Tests for the IViewAware lifecycle managed by RegionManagerBase.OnNavigated.
/// Platform-specific context (TopLevel/Window) is replaced with TestViewContext in the test RegionManager.
/// </summary>
public class IViewAwareTests : IClassFixture<ServiceFixture>
{
    private readonly IRegionManager _regionManager;
    private readonly IServiceProvider _serviceProvider;

    public IViewAwareTests(ServiceFixture serviceFixture)
    {
        _serviceProvider = serviceFixture.ServiceProvider;
        _regionManager = _serviceProvider.GetRequiredService<IRegionManager>();
    }

    private TestRegion RegisterRegion(string name, bool isSinglePage)
    {
        _regionManager.TryRemoveRegion(name, out _);
        var region = new TestRegion(name, new object(), _serviceProvider, isSinglePage);
        _regionManager.AddRegion(name, region);
        return region;
    }

    private static TestViewAwareNavigationAware? GetViewModel(NavigationResult result)
        => result.NavigationContext?.Target.Value?.DataContext as TestViewAwareNavigationAware;

    // -------------------------------------------------------------------------
    // Attach
    // -------------------------------------------------------------------------

    [Fact]
    public async Task OnViewAttached_ShouldBeCalled_WithNonNullContext_AfterNavigation()
    {
        RegisterRegion("Va_Attach", isSinglePage: false);

        var result = await _regionManager.RequestNavigateAsync("Va_Attach", "ViewAwareView");

        Assert.True(result.IsSuccessful);
        var vm = GetViewModel(result);
        Assert.NotNull(vm);
        Assert.Equal(1, vm.AttachedCount);
        Assert.IsType<TestViewContext>(vm.LastViewContext);
    }

    [Fact]
    public async Task OnViewAttached_ShouldBeCalled_EachNavigation_WhenViewModelIsNewInstance()
    {
        RegisterRegion("Va_EachNav", isSinglePage: false);

        var r1 = await _regionManager.RequestNavigateAsync("Va_EachNav", "ViewAwareView");
        var r2 = await _regionManager.RequestNavigateAsync("Va_EachNav", "ViewAwareView");

        var vm1 = GetViewModel(r1);
        var vm2 = GetViewModel(r2);

        // IsNavigationTargetAsync returns false, so each navigation creates a fresh ViewModel.
        Assert.NotSame(vm1, vm2);
        Assert.Equal(1, vm1!.AttachedCount);
        Assert.Equal(1, vm2!.AttachedCount);
    }

    // -------------------------------------------------------------------------
    // Detach — single-page region
    // -------------------------------------------------------------------------

    [Fact]
    public async Task OnViewDetached_ShouldBeCalled_WhenNavigatingAway_InSinglePageRegion()
    {
        RegisterRegion("Va_SingleDetach", isSinglePage: true);

        var r1 = await _regionManager.RequestNavigateAsync("Va_SingleDetach", "ViewAwareView");
        var vm1 = GetViewModel(r1);
        Assert.NotNull(vm1);
        Assert.Equal(0, vm1.DetachedCount);

        // Navigate to a non-IViewAware view — should trigger detach of vm1.
        await _regionManager.RequestNavigateAsync("Va_SingleDetach", "TestView");

        Assert.Equal(1, vm1.DetachedCount);
    }

    [Fact]
    public async Task OnViewDetached_ShouldNotBeCalled_WhenNavigatingAway_InMultiPageRegion()
    {
        RegisterRegion("Va_MultiNoDetach", isSinglePage: false);

        var r1 = await _regionManager.RequestNavigateAsync("Va_MultiNoDetach", "ViewAwareView");
        var vm1 = GetViewModel(r1);
        Assert.NotNull(vm1);

        // Add a second view in the same multi-page region — vm1 must remain active.
        await _regionManager.RequestNavigateAsync("Va_MultiNoDetach", "TestView");

        Assert.Equal(0, vm1.DetachedCount);
    }

    [Fact]
    public async Task OnViewDetached_ShouldBeCalled_ForPreviousViewOnly_InSinglePageRegion()
    {
        RegisterRegion("Va_SingleReplace", isSinglePage: true);

        var r1 = await _regionManager.RequestNavigateAsync("Va_SingleReplace", "ViewAwareView");
        var vm1 = GetViewModel(r1);

        var r2 = await _regionManager.RequestNavigateAsync("Va_SingleReplace", "ViewAwareView");
        var vm2 = GetViewModel(r2);

        Assert.NotSame(vm1, vm2);
        Assert.Equal(1, vm1!.DetachedCount);   // evicted ViewModel
        Assert.Equal(0, vm2!.DetachedCount);   // currently active ViewModel
        Assert.Equal(1, vm2.AttachedCount);
    }

    // -------------------------------------------------------------------------
    // Non-IViewAware ViewModel — no errors, navigation succeeds
    // -------------------------------------------------------------------------

    [Fact]
    public async Task Navigation_WithNonIViewAwareViewModel_ShouldSucceedWithoutErrors()
    {
        RegisterRegion("Va_NonAware", isSinglePage: true);

        var result = await _regionManager.RequestNavigateAsync("Va_NonAware", "TestView");

        Assert.True(result.IsSuccessful);
    }
}
