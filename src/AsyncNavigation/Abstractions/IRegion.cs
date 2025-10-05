﻿using AsyncNavigation.Core;

namespace AsyncNavigation.Abstractions;

public interface IRegion : IDisposable
{
    string Name { get; }
    internal IRegionPresenter RegionPresenter { get; }
    Task NavigateFromAsync(NavigationContext navigationContext);
    Task ActivateViewAsync(NavigationContext navigationContext);
    Task<bool> CanGoBackAsync();
    Task GoBackAsync(CancellationToken cancellationToken = default);
    Task<bool> CanGoForwardAsync();
    Task GoForwardAsync(CancellationToken cancellationToken = default);
}
