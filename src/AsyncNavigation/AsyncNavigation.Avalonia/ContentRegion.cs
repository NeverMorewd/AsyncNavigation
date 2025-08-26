using AsyncNavigation.Abstractions;
using AsyncNavigation.Core;
using Avalonia.Controls;
using Microsoft.Extensions.DependencyInjection;

namespace AsyncNavigation.Avalonia;

public class ContentRegion : IRegion, IRegionPresenter
{
    private readonly ContentControl _contentControl;
    private readonly IRegionNavigationService<ContentRegion> _regionNavigationService;
    public ContentRegion(ContentControl contentControl, IServiceProvider serviceProvider, bool? useCache)
    {
        ArgumentNullException.ThrowIfNull(contentControl);
        ArgumentNullException.ThrowIfNull(serviceProvider);
        _contentControl = contentControl;
        EnableViewCache = useCache ?? true;
        var factory = serviceProvider.GetRequiredService<IRegionNavigationServiceFactory>();
        _regionNavigationService = factory.Create(this);
    }
    IRegionPresenter IRegion.RegionPresenter => this;
    public INavigationHistory NavigationHistory => throw new NotImplementedException();
    public bool EnableViewCache { get; }
    public bool IsSinglePageRegion => true;
    #region IRegion Methods
    public async Task<NavigationResult> ActivateViewAsync(NavigationContext navigationContext)
    {
        return await _regionNavigationService.RequestNavigateAsync(navigationContext);
    }
    public Task<bool> CanGoBackAsync()
    {
        throw new NotImplementedException();
    }

    public Task<NavigationResult> GoBackAsync(CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<bool> CanGoForwardAsync()
    {
        throw new NotImplementedException();
    }

    public Task<NavigationResult> GoForwardAsync(CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
    public void Dispose()
    {
        GC.SuppressFinalize(this);
        _contentControl.Content = null;
    }
    #endregion

    public void RenderIndicator(NavigationContext navigationContext)
    {
        _contentControl.Content = navigationContext.Indicator.Value!.IndicatorControl;
    }
    public void ProcessActivate(NavigationContext navigationContext)
    {
        _contentControl.Content = navigationContext.Indicator.Value!.IndicatorControl;
    }
    public void ProcessDeactivate(NavigationContext navigationContext)
    {
        _contentControl.Content = null;
    }
}
