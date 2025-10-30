using AsyncNavigation.Abstractions;

namespace AsyncNavigation.Tests.Mocks;

internal class TestInnerRegionIndicatorHost : IInnerRegionIndicatorHost
{
    public object Host => throw new NotImplementedException();

    public Task OnCancelledAsync(NavigationContext context)
    {
        throw new NotImplementedException();
    }

    public Task OnLoadedAsync(NavigationContext context)
    {
        throw new NotImplementedException();
    }

    public Task ShowContentAsync(NavigationContext context)
    {
        throw new NotImplementedException();
    }

    public Task ShowErrorAsync(NavigationContext context, Exception? innerException = null)
    {
        throw new NotImplementedException();
    }

    public Task ShowLoadingAsync(NavigationContext context)
    {
        throw new NotImplementedException();
    }
}
