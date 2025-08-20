using AsyncNavigation.Abstractions;
using AsyncNavigation.Core;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace AsyncNavigation.Avalonia;

public class TabRegion : IRegion
{
    private readonly TabControl _tabControl;
    private readonly IRegionNavigationService<TabRegion> _regionNavigationService;
    private readonly TabRegionState _state = new();
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
           new Binding(nameof(TabRegionState.Tabs)) { Source = _state });

        _tabControl.Bind(
            SelectingItemsControl.SelectedItemProperty,
            new Binding(nameof(TabRegionState.Selected)) { Source = _state, Mode = BindingMode.TwoWay });

        _tabControl.ContentTemplate = new FuncDataTemplate<NavigationContext>((context, _) => 
        {
            if (context != null && context.Indicator.Value is IRegionIndicator regionIndicator)
                return regionIndicator.IndicatorControl as Control;
            return null;
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
        var hit = _state.Tabs.FirstOrDefault(t => ReferenceEquals(t, navigationContext));
        if (hit != null)
        {
            _state.Selected = hit;
        }
    }

    public void ProcessDeactivate(NavigationContext navigationContext)
    {
        var hit = _state.Tabs.FirstOrDefault(t => ReferenceEquals(t, navigationContext));
        if (hit != null)
        {
            bool wasSelected = ReferenceEquals(_state.Selected, hit);
            _state.Tabs.Remove(hit);
            if (wasSelected)
                _state.Selected = _state.Tabs.FirstOrDefault();
        }
    }

    public void RenderIndicator(NavigationContext navigationContext, IRegionIndicator regionIndicator)
    {
        if (!_state.Tabs.Contains(navigationContext))
            _state.Tabs.Add(navigationContext);

        ProcessActivate(navigationContext);
    }
}

public sealed class TabRegionState : INotifyPropertyChanged
{
    public ObservableCollection<NavigationContext> Tabs { get; } = [];

    private NavigationContext? _selected;
    public NavigationContext? Selected
    {
        get => _selected;
        set
        {
            if (!ReferenceEquals(_selected, value))
            {
                _selected = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Selected)));
            }
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;
}


