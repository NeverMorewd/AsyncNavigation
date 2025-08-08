using AsyncNavigation.Abstractions;
using AsyncNavigation.Core;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Microsoft.Extensions.DependencyInjection;
using System.ComponentModel;

namespace AsyncNavigation.Avalonia;

public class ContentRegion : IContentRegion<ContentControl>, IRegionProcessor
{
    private readonly ContentControl _contentControl;
    private readonly IServiceProvider _serviceProvider;
    //private readonly ViewTemplateSelector _viewTemplateSelector;
    private readonly IRegionNavigationService<ContentRegion> _regionNavigationService;

    #region IRegion Propertries
    public string Name { get; }

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
    public ContentRegion(string name, ContentControl contentControl, IServiceProvider serviceProvider)
    {
        Name = name;
        _serviceProvider = serviceProvider;
        _contentControl = contentControl;
        _regionNavigationService = _serviceProvider.GetRequiredService<IRegionNavigationService<ContentRegion>>();
        //_viewTemplateSelector = new ViewTemplateSelector(_serviceProvider);
        //_contentControl.DataTemplates.Add(_viewTemplateSelector);
        _regionNavigationService.Setup(this);
    }
    public ContentControl ContentControl => _contentControl;
    
    #region IRegion Methods
    public async Task<NavigationResult> ActivateViewAsync(NavigationContext navigationContext)
    {
        try
        {
            await _regionNavigationService.RequestNavigateAsync(navigationContext);
            //Content = navigationContext;
            //await _viewTemplateSelector.WaitAsync(navigationContext);
            return NavigationResult.Successful();
        }
        catch (Exception ex)
        {
            return NavigationResult.Failed(ex.Message, ex);
        }
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

    public void ProcessActivate(NavigationContext navigationContext, object content)
    {
        _contentControl.Content = content;
    }

    public void ProcessDeactivate(NavigationContext navigationContext, object content)
    {
        _contentControl.Content = null;
    }
}
