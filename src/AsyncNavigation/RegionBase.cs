using AsyncNavigation.Abstractions;
using AsyncNavigation.Core;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics;
using System.Security.Cryptography;

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
    async Task IRegion.ActivateViewAsync(NavigationContext navigationContext)
    {
        await _regionNavigationService.RequestNavigateAsync(navigationContext);
        _navigationHistory.Add(navigationContext);
    }


    Task<bool> IRegion.CanGoBackAsync()
    {
        return Task.FromResult(_navigationHistory.CanGoBack);
    }

    public async Task GoBackAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var context = _navigationHistory.GoBack() ?? throw new NavigationException("Cannot go back!");
        context.IsBackNavigation = true;
        context.CancellationToken = cancellationToken;
        await _regionNavigationService.RequestNavigateAsync(context);
    }

    Task<bool> IRegion.CanGoForwardAsync()
    {
        return Task.FromResult(_navigationHistory.CanGoForward);
    }

    public async Task GoForwardAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var context = _navigationHistory.GoForward() ?? throw new NavigationException("Cannot go forward!");
        context.IsForwordNavigation = true;
        context.CancellationToken = cancellationToken;
        await _regionNavigationService.RequestNavigateAsync(context);
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

#if DEBUG
    ~RegionBase()
    {
        Debug.WriteLine($"{Name} was collected!");
    }
#endif
}
