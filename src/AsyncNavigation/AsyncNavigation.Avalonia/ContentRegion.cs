using AsyncNavigation.Abstractions;
using AsyncNavigation.Core;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Microsoft.Extensions.DependencyInjection;
using System.ComponentModel;

namespace AsyncNavigation.Avalonia;

public class ContentRegion : IRegion
{
    private readonly ContentControl _contentControl;
    private readonly IServiceProvider _serviceProvider;
    private readonly IRegionNavigationService<ContentRegion> _regionNavigationService;

    #region IRegion Propertries
    public string Name { get; } = null!;

    public INavigationAware? ActiveView => throw new NotImplementedException();

    public INavigationHistory NavigationHistory => throw new NotImplementedException();

    public bool IsInitialized => throw new NotImplementedException();

    public IServiceProvider? ServiceProvider { get; set; }

    IReadOnlyCollection<IView> IRegion.Views => throw new NotImplementedException();

    public event PropertyChangedEventHandler? PropertyChanged;
    public event AsyncEventHandler<ViewActivatedEventArgs<INavigationAware>>? ViewActivated;
    public event AsyncEventHandler<ViewDeactivatedEventArgs<INavigationAware>>? ViewDeactivated;
    public event AsyncEventHandler<ViewAddedEventArgs<INavigationAware>>? ViewAdded;
    public event AsyncEventHandler<ViewRemovedEventArgs<INavigationAware>>? ViewRemoved;
    public event AsyncEventHandler<NavigationFailedEventArgs>? NavigationFailed;
    #endregion
    public ContentRegion(ContentControl contentControl, IServiceProvider serviceProvider, bool? useCache)
    {
        ArgumentNullException.ThrowIfNull(contentControl);
        ArgumentNullException.ThrowIfNull(serviceProvider);

        _serviceProvider = serviceProvider;
        _contentControl = contentControl;

        EnableViewCache = useCache ?? true;

        var factory = serviceProvider.GetRequiredService<IRegionNavigationServiceFactory>();
        _regionNavigationService = factory.Create(this);
    }

    public ContentControl ContentControl => _contentControl;
    public bool EnableViewCache { get; }
    public bool IsSinglePageRegion => true;
    #region IRegion Methods
    public async Task<NavigationResult> ActivateViewAsync(NavigationContext navigationContext)
    {
        return await _regionNavigationService.RequestNavigateAsync(navigationContext);
    }

    public Task<NavigationResult> DeactivateViewAsync(NavigationContext navigationContext)
    {
        throw new NotImplementedException();
    }

    public Task<bool> CanNavigateAsync(NavigationContext context)
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

    public ValueTask DisposeAsync()
    {
        throw new NotImplementedException();
    }
    #endregion

    public bool AddView(IView view)
    {
        var viewTypeName = view.GetType().FullName;
        var template = new FuncDataTemplate<NavigationContext>((context, np) =>
        {
            view.DataContext = context;
            return view as Control;
        }, true);
        //_viewTemplateSelector.RegisterTemplate(viewTypeName!, template);
        return true;
    }

    public bool RemoveView(IView view)
    {
        throw new NotImplementedException();
    }

    public bool ContainsView(IView view)
    {
        throw new NotImplementedException();
    }

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

    public void Dispose()
    {
        throw new NotImplementedException();
    }
}
