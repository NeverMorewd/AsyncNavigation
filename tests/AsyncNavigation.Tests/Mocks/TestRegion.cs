using AsyncNavigation.Abstractions;
using AsyncNavigation.Core;

namespace AsyncNavigation.Tests.Mocks;

public class TestRegion : IRegion
{
    public string Name => throw new NotImplementedException();

    IRegionPresenter IRegion.RegionPresenter => throw new NotImplementedException();

    public Task ActivateViewAsync(NavigationContext navigationContext)
    {
        return Task.CompletedTask;
    }

    public Task<bool> CanGoBackAsync()
    {
        throw new NotImplementedException();
    }

    public Task<bool> CanGoForwardAsync()
    {
        throw new NotImplementedException();
    }

    public void Dispose()
    {
        throw new NotImplementedException();
    }

    public Task GoBackAsync(CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task GoForwardAsync(CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task NavigateFromAsync(NavigationContext navigationContext)
    {
        throw new NotImplementedException();
    }
}
