using AsyncNavigation.Abstractions;
using AsyncNavigation.Core;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Microsoft.Extensions.DependencyInjection;

namespace AsyncNavigation.Avalonia;

public class ItemsRegion : IRegion, IRegionPresenter
{
    private readonly IRegionNavigationService<ItemsRegion> _regionNavigationService;
    private readonly ItemsControl _itemsControl;
    private readonly ItemsRegionContext _context = new();
    public ItemsRegion(ItemsControl itemsControl, IServiceProvider serviceProvider, bool? useCache)
    {
        ArgumentNullException.ThrowIfNull(itemsControl);
        ArgumentNullException.ThrowIfNull(serviceProvider);

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
    IRegionPresenter IRegion.RegionPresenter => this;
    public INavigationHistory NavigationHistory => throw new NotImplementedException();

    public async Task<NavigationResult> ActivateViewAsync(NavigationContext navigationContext)
    {
        return await _regionNavigationService.RequestNavigateAsync(navigationContext);
    }

    public bool AddView(IView view)
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

    public Task<NavigationResult> GoBackAsync(CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<NavigationResult> GoForwardAsync(CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
    public void Dispose()
    {
        GC.SuppressFinalize(this);
        _context.Clear();
    }
    public virtual void ProcessActivate(NavigationContext navigationContext)
    {
        _itemsControl.ScrollIntoView(navigationContext);
    }
    public virtual void RenderIndicator(NavigationContext navigationContext)
    {
        if (!_context.Items.Contains(navigationContext))
            _context.Items.Add(navigationContext);
        ProcessActivate(navigationContext);
    }

    public virtual void ProcessDeactivate(NavigationContext navigationContext)
    {       
        _context.Items.Remove(navigationContext);
    }
}
