using AsyncNavigation.Abstractions;
using AsyncNavigation.Core;
using Avalonia.Controls;
using Microsoft.Extensions.DependencyInjection;

namespace AsyncNavigation.Avalonia;

public class TabRegion : IRegion
{
    private readonly TabControl _tabControl;
    private readonly IRegionNavigationService<TabRegion> _regionNavigationService;
    public TabRegion(string name, 
        TabControl control, 
        IServiceProvider serviceProvider, 
        bool? useCache = null)
    {
        _tabControl = control;
        EnableViewCache = useCache ?? false;
        var factory = serviceProvider.GetRequiredService<IRegionNavigationServiceFactory>();
        _regionNavigationService = factory.Create(this);
    }
    public string Name => throw new NotImplementedException();

    public INavigationAware? ActiveView => throw new NotImplementedException();

    public IReadOnlyCollection<IView> Views => throw new NotImplementedException();

    public INavigationHistory NavigationHistory => throw new NotImplementedException();

    public bool IsInitialized => throw new NotImplementedException();

    public bool EnableViewCache { get; }

    public bool IsSinglePageRegion => false;

    public event AsyncEventHandler<ViewActivatedEventArgs<INavigationAware>>? ViewActivated;
    public event AsyncEventHandler<ViewDeactivatedEventArgs<INavigationAware>>? ViewDeactivated;
    public event AsyncEventHandler<ViewAddedEventArgs<INavigationAware>>? ViewAdded;
    public event AsyncEventHandler<ViewRemovedEventArgs<INavigationAware>>? ViewRemoved;
    public event AsyncEventHandler<NavigationFailedEventArgs>? NavigationFailed;

    public Task<NavigationResult> ActivateViewAsync(NavigationContext navigationContext)
    {
        return _regionNavigationService.RequestNavigateAsync(navigationContext);
    }

    public bool AddView(IView view)
    {
        throw new NotImplementedException();
    }

    public Task<bool> CanDeactivateAsync(CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<bool> CanGoBackAsync()
    {
        throw new NotImplementedException();
    }

    public Task<bool> CanGoForwardAsync()
    {
        throw new NotImplementedException();
    }

    public Task<bool> CanNavigateAsync(NavigationContext context)
    {
        throw new NotImplementedException();
    }

    public bool ContainsView(IView view)
    {
        throw new NotImplementedException();
    }

    public Task<NavigationResult> DeactivateViewAsync(NavigationContext navigationContext)
    {
        throw new NotImplementedException();
    }

    public ValueTask DisposeAsync()
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

    public Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public void ProcessActivate(NavigationContext navigationContext)
    {
    }

    public void ProcessDeactivate(NavigationContext navigationContext)
    {
        throw new NotImplementedException();
    }

    public bool RemoveView(IView view)
    {
        throw new NotImplementedException();
    }

    public void RenderIndicator(NavigationContext navigationContext, IRegionIndicator regionIndicator)
    {
        if (!_tabControl.Items.OfType<TabItem>().Any(ti => ti.Tag == navigationContext))
        {
            var tabItem = new TabItem
            {
                Header = navigationContext.ViewName,
                Content = (navigationContext.Indicator.Value as IRegionIndicator)?.IndicatorControl,
                Tag = navigationContext
            };
            _tabControl.Items.Add(tabItem);
        }

        _tabControl.SelectedItem = _tabControl.Items.OfType<TabItem>().First(ti => ti.Tag == navigationContext);
    }
}
