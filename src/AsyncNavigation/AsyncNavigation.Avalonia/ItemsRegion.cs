using AsyncNavigation.Abstractions;
using AsyncNavigation.Core;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.ObjectModel;

namespace AsyncNavigation.Avalonia;

public class ItemsRegion : IRegion
{
    private readonly IRegionNavigationService<ItemsRegion> _regionNavigationService;
    private readonly ItemsControl _itemsControl;
    private readonly ItemsRegionContext _context = new();
    public ItemsRegion(ItemsControl itemsControl, IServiceProvider serviceProvider, bool? useCache)
    {
        _itemsControl = itemsControl;
        _itemsControl.ItemTemplate = new FuncDataTemplate<NavigationContext>((context, np) =>
        {
            return context?.Indicator.Value?.IndicatorControl as Control;
        });

        _itemsControl.Bind(
           ItemsControl.ItemsSourceProperty,
           new Binding(nameof(ItemsRegionContext.Items)) { Source = _context });

        _itemsControl.Bind(
            SelectingItemsControl.SelectedItemProperty,
            new Binding(nameof(ItemsRegionContext.Selected)) { Source = _context, Mode = BindingMode.TwoWay });
        
        EnableViewCache = useCache ?? false;
        var factory = serviceProvider.GetRequiredService<IRegionNavigationServiceFactory>();
        _regionNavigationService = factory.Create(this);
    }
    public bool EnableViewCache { get; }
    public bool IsSinglePageRegion => false;
    public ObservableCollection<NavigationContext> Contexts => throw new NotImplementedException();

    public ItemsControl ItemsControl => throw new NotImplementedException();

    public IServiceProvider? ServiceProvider { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

    public string Name => throw new NotImplementedException();

    public INavigationAware? ActiveView => throw new NotImplementedException();

    public IReadOnlyCollection<IView> Views => throw new NotImplementedException();

    public INavigationHistory NavigationHistory => throw new NotImplementedException();

    public bool IsInitialized => throw new NotImplementedException();

    public event AsyncEventHandler<ViewActivatedEventArgs<INavigationAware>>? ViewActivated;
    public event AsyncEventHandler<ViewDeactivatedEventArgs<INavigationAware>>? ViewDeactivated;
    public event AsyncEventHandler<ViewAddedEventArgs<INavigationAware>>? ViewAdded;
    public event AsyncEventHandler<ViewRemovedEventArgs<INavigationAware>>? ViewRemoved;
    public event AsyncEventHandler<NavigationFailedEventArgs>? NavigationFailed;


    public async Task<NavigationResult> ActivateViewAsync(NavigationContext navigationContext)
    {
        return await _regionNavigationService.RequestNavigateAsync(navigationContext);
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
        _itemsControl.ScrollIntoView(navigationContext);
    }
    public void RenderIndicator(NavigationContext navigationContext)
    {
        if (!_context.Items.Contains(navigationContext))
            _context.Items.Add(navigationContext);
        ProcessActivate(navigationContext);
    }

    public void ProcessDeactivate(NavigationContext navigationContext)
    {       
        _context.Items.Remove(navigationContext);
    }

    public bool RemoveView(IView view)
    {
        throw new NotImplementedException();
    }

    public void Dispose()
    {
        throw new NotImplementedException();
    }
}
