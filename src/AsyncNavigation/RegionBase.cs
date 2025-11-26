using AsyncNavigation.Abstractions;
using AsyncNavigation.Core;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics;

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

        RegionControlAccessor.ExecuteOn(InitializeOnRegionCreated);

    }

    IRegionPresenter IRegion.RegionPresenter => this;
    public bool EnableViewCache { get; protected set; }
    public bool IsSinglePageRegion { get; protected set; }
    public abstract NavigationPipelineMode NavigationPipelineMode { get; }
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

    /// <summary>
    /// Performs initialization logic when a region is created and associated with the specified control.
    /// Binding logic or setup tasks related to the control can be implemented in this method.
    /// </summary>
    /// <remarks>This method is intended to be overridden in a derived class to provide custom initialization
    /// logic. The base implementation does not perform any actions.</remarks>
    /// <param name="control">The control associated with the newly created region. This parameter cannot be null.</param>
    protected virtual void InitializeOnRegionCreated(TControl control)
    {
        
    }
    public async Task GoBackAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var context = _navigationHistory.GoBack() ?? throw new NavigationException("Cannot go back!");
        context.IsBackNavigation = true;
        context.LinkCancellationToken(cancellationToken);
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
        context.LinkCancellationToken(cancellationToken);
        await _regionNavigationService.RequestNavigateAsync(context);
    }
    Task IRegion.NavigateFromAsync(NavigationContext navigationContext)
    {
        return _regionNavigationService.OnNavigateFromAsync(navigationContext);
    }

    Task IRegion.RevertAsync(NavigationContext? navigationContext)
    {
        return _regionNavigationService.RevertAsync(navigationContext);
    }
    public virtual void Dispose()
    {
        GC.SuppressFinalize(this);
        _navigationHistory.Clear();
    }
    #endregion
    public abstract void ProcessActivate(NavigationContext navigationContext);
    public abstract void ProcessDeactivate(NavigationContext? navigationContext);

#if DEBUG
    ~RegionBase()
    {
        Debug.WriteLine($"{Name} was collected!");
    }
#endif
}
