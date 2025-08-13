using AsyncNavigation.Abstractions;
using AsyncNavigation.Core;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.ObjectModel;

namespace AsyncNavigation.Avalonia;

public class ItemsRegion : IItemsRegion<ItemsControl>
{
    private readonly IRegionNavigationService<ItemsRegion> _regionNavigationService;
    private readonly ItemsControl _itemsControl;
    public ItemsRegion(string name, ItemsControl itemsControl, IServiceProvider serviceProvider, bool? useCache)
    {
        _itemsControl = itemsControl;
        _itemsControl.ItemTemplate = new FuncDataTemplate<NavigationContext>((context, np) =>
        {
            if(context.Indicator.Value is IRegionIndicator regionIndicator)
            {
                return regionIndicator.Control as Control;
            }
            return null;
        });
        if (useCache != null)
        {
            EnableViewCache = useCache.Value;
        }
        else
        {
            EnableViewCache = false;
        }
        _regionNavigationService = serviceProvider.GetRequiredService<IRegionNavigationService<ItemsRegion>>();
        _regionNavigationService.SeRegionProcessor(this);
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
        if (_itemsControl.Items.Contains(navigationContext))
        {

        }
        else
        {
            _itemsControl.Items.Add(navigationContext);
        }
        _itemsControl.ScrollIntoView(navigationContext);
    }
    public void RenderIndicator(NavigationContext navigationContext, IRegionIndicator regionIndicator)
    {
        if (_itemsControl.Items.Contains(navigationContext))
        {

        }
        else
        {
            _itemsControl.Items.Add(navigationContext);
        }
        _itemsControl.ScrollIntoView(navigationContext);
    }

    public void ProcessDeactivate(NavigationContext navigationContext)
    {
        throw new NotImplementedException();
    }

    public bool RemoveView(IView view)
    {
        throw new NotImplementedException();
    }
}
