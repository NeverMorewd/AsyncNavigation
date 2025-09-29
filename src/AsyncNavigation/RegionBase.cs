using AsyncNavigation.Abstractions;
using AsyncNavigation.Core;
using Microsoft.Extensions.DependencyInjection;

namespace AsyncNavigation;

public abstract class RegionBase<TRegion, TControl> : IRegion, IRegionPresenter
    where TRegion : class, IRegionPresenter
    where TControl : class
{
    private readonly IRegionNavigationService<TRegion> _regionNavigationService;
    private readonly IRegionNavigationHistory _navigationHistory;
    private readonly IRegionControlAccessor<TControl> _controlAccessor;
    protected readonly RegionContext _context = new();
    public RegionBase(string name, TControl control, IServiceProvider serviceProvider)
    {
        ArgumentNullException.ThrowIfNull(control);
        ArgumentNullException.ThrowIfNull(serviceProvider);
        Name = name;
        _controlAccessor = new WeakRegionControlAccessor<TControl>(control);
        _regionNavigationService = serviceProvider.GetRequiredService<IRegionNavigationServiceFactory>().Create((this as TRegion)!);
        _navigationHistory = serviceProvider.GetRequiredService<IRegionNavigationHistory>();
    }

    IRegionPresenter IRegion.RegionPresenter => this;
    public bool EnableViewCache { get; protected set; }
    public bool IsSinglePageRegion { get; protected set; }

    public IRegionControlAccessor<TControl> RegionControlAccessor => _controlAccessor;

    public string Name
    {
        get;
        protected set;
    }
    #region IRegion Methods
    async Task<NavigationResult> IRegion.ActivateViewAsync(NavigationContext navigationContext)
    {
        var result = await _regionNavigationService.RequestNavigateAsync(navigationContext);
        if (result.IsSuccessful)
        {
            _navigationHistory.Add(navigationContext);
        }
        return result;
    }

    public Task<bool> CanGoBackAsync()
    {
        return Task.FromResult(_navigationHistory.CanGoBack);
    }

    public async Task<NavigationResult> GoBackAsync(CancellationToken cancellationToken = default)
    {
        if (await CanGoBackAsync())
        {
            var context = _navigationHistory.GoBack();
            context!.CancellationToken = cancellationToken;
            context.IsBackNavigation = true;
            return await _regionNavigationService.RequestNavigateAsync(context);
        }
        return NavigationResult.Failure(new NavigationException("Can not go back!"), TimeSpan.Zero);
    }

    public Task<bool> CanGoForwardAsync()
    {
        return Task.FromResult(_navigationHistory.CanGoForward);
    }

    public async Task<NavigationResult> GoForwardAsync(CancellationToken cancellationToken = default)
    {
        if (await CanGoForwardAsync())
        {
            var context = _navigationHistory.GoForward();
            context!.CancellationToken = cancellationToken;
            context.IsForwordNavigation = true;
            return await _regionNavigationService.RequestNavigateAsync(context);
        }
        return NavigationResult.Failure(new NavigationException("Can not go forward!"), TimeSpan.Zero);
    }
    Task IRegion.NavigateFromAsync(NavigationContext navigationContext)
    {
        return _regionNavigationService.OnNavigateFromAsync(navigationContext);
    }
    public virtual void Dispose()
    {
        GC.SuppressFinalize(this);
        _navigationHistory.Clear();
    }
    #endregion

    public abstract void RenderIndicator(NavigationContext navigationContext);
    public abstract void ProcessActivate(NavigationContext navigationContext);
    public abstract void ProcessDeactivate(NavigationContext navigationContext);
}
