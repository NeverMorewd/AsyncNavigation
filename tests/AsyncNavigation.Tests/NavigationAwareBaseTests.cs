using AsyncNavigation.Abstractions;
using AsyncNavigation.Core;

namespace AsyncNavigation.Tests;

public class NavigationAwareBaseTests
{
    private sealed class TestAware : NavigationAwareBase
    {
        public Task RequestUnload(CancellationToken ct = default) => RequestUnloadAsync(ct);
    }

    private static NavigationContext MakeContext() => new() { RegionName = "R", ViewName = "V" };

    [Fact]
    public async Task InitializeAsync_DefaultImplementation_ReturnsCompleted()
    {
        var vm = new TestAware();
        await vm.InitializeAsync(MakeContext());
    }

    [Fact]
    public async Task OnNavigatedToAsync_DefaultImplementation_ReturnsCompleted()
    {
        var vm = new TestAware();
        await vm.OnNavigatedToAsync(MakeContext());
    }

    [Fact]
    public async Task OnNavigatedFromAsync_DefaultImplementation_ReturnsCompleted()
    {
        var vm = new TestAware();
        await vm.OnNavigatedFromAsync(MakeContext());
    }

    [Fact]
    public async Task IsNavigationTargetAsync_DefaultImplementation_ReturnsTrue()
    {
        var vm = new TestAware();
        var result = await vm.IsNavigationTargetAsync(MakeContext());
        Assert.True(result);
    }

    [Fact]
    public async Task OnUnloadAsync_DefaultImplementation_ReturnsCompleted()
    {
        var vm = new TestAware();
        await vm.OnUnloadAsync(CancellationToken.None);
    }

    [Fact]
    public async Task RequestUnloadAsync_WithNoSubscribers_ReturnsCompleted()
    {
        var vm = new TestAware();
        await vm.RequestUnload();
    }

    [Fact]
    public async Task RequestUnloadAsync_WithSubscriber_RaisesEvent()
    {
        var vm = new TestAware();
        bool raised = false;
        vm.AsyncRequestUnloadEvent += (_, _) => { raised = true; return Task.CompletedTask; };

        await vm.RequestUnload();

        Assert.True(raised);
    }

    [Fact]
    public async Task RequestUnloadAsync_PassesCancellationToken()
    {
        var vm = new TestAware();
        using var cts = new CancellationTokenSource();
        CancellationToken received = default;

        vm.AsyncRequestUnloadEvent += (_, args) =>
        {
            received = args.CancellationToken;
            return Task.CompletedTask;
        };

        await vm.RequestUnload(cts.Token);

        Assert.Equal(cts.Token, received);
    }
}
