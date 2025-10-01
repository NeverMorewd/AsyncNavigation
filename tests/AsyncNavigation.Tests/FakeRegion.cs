using AsyncNavigation.Abstractions;
using AsyncNavigation.Core;
using System.Threading;
using System.Threading.Tasks;

namespace AsyncNavigation.Tests;

public class FakeRegion : IRegion
{
    public string Name => throw new NotImplementedException();

    IRegionPresenter IRegion.RegionPresenter => throw new NotImplementedException();

    public Task<NavigationResult> ActivateViewAsync(NavigationContext navigationContext)
    {
        return Task.FromResult(NavigationResult.Success(TimeSpan.Zero));
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

    public Task<NavigationResult> GoBackAsync(CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<NavigationResult> GoForwardAsync(CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task NavigateFromAsync(NavigationContext navigationContext)
    {
        throw new NotImplementedException();
    }
}
