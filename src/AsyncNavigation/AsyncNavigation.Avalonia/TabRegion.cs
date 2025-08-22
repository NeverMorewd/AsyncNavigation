using AsyncNavigation.Abstractions;
using AsyncNavigation.Core;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Microsoft.Extensions.DependencyInjection;

namespace AsyncNavigation.Avalonia;

public class TabRegion : IRegion
{
    private readonly TabControl _tabControl;
    private readonly IRegionNavigationService<TabRegion> _regionNavigationService;
    private readonly ItemsRegionContext _context = new();
    public TabRegion(TabControl control,
        IServiceProvider serviceProvider,
        bool? useCache = null)
    {
        _tabControl = control;
        EnableViewCache = useCache ?? false;
        var factory = serviceProvider.GetRequiredService<IRegionNavigationServiceFactory>();
        _regionNavigationService = factory.Create(this);

        _tabControl.Bind(
           ItemsControl.ItemsSourceProperty,
           new Binding(nameof(ItemsRegionContext.Items)) { Source = _context });

        _tabControl.Bind(
            SelectingItemsControl.SelectedItemProperty,
            new Binding(nameof(ItemsRegionContext.Selected)) { Source = _context, Mode = BindingMode.TwoWay });

        _tabControl.ContentTemplate = new FuncDataTemplate<NavigationContext>((context, _) => 
        {
            return context.Indicator.Value!.IndicatorControl as Control;
        });
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
    public bool RemoveView(IView view)
    {
        throw new NotImplementedException();
    }


    public void ProcessActivate(NavigationContext navigationContext)
    {
        var hit = _context.Items.FirstOrDefault(t => t.Equals(navigationContext));
        if (hit != null)
        {
            _context.Selected = hit;
        }
    }

    public void ProcessDeactivate(NavigationContext navigationContext)
    {
        var hit = _context.Items.FirstOrDefault(t => ReferenceEquals(t, navigationContext));
        if (hit != null)
        {
            bool wasSelected = ReferenceEquals(_context.Selected, hit);
            _context.Items.Remove(hit);
            if (wasSelected)
                _context.Selected = _context.Items.FirstOrDefault();
        }
    }

    public void RenderIndicator(NavigationContext navigationContext)
    {
        if (!_context.Items.Contains(navigationContext))
            _context.Items.Add(navigationContext);

        ProcessActivate(navigationContext);
    }

    public void Dispose()
    {
        throw new NotImplementedException();
    }
}


