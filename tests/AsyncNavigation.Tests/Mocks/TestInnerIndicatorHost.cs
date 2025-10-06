using AsyncNavigation.Abstractions;

namespace AsyncNavigation.Tests.Mocks;

internal class TestInnerIndicatorHost : IInnerRegionIndicatorHost
{
    public object Host => new object();

    public Task OnCancelledAsync(NavigationContext context)
    {
        return Task.CompletedTask;
    }

    public Task OnLoadedAsync(NavigationContext context)
    {
        return Task.CompletedTask;
    }

    public Task ShowContentAsync(NavigationContext context)
    {
        return Task.CompletedTask;
    }

    public Task ShowErrorAsync(NavigationContext context, Exception? innerException = null)
    {
        return Task.CompletedTask;
    }

    public Task ShowLoadingAsync(NavigationContext context)
    {
        return Task.CompletedTask;
    }
}
