using AsyncNavigation;
using AsyncNavigation.Abstractions;
using AsyncNavigation.Core;
using AsyncNavigation.Floating;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Sample.Common;

public partial class LightViewModel : InstanceCounterViewModel<LightViewModel>, IDialogAware, INavigationMetadata, INavigationGuard
{
    private readonly IRegionManager _regionManager;
    private readonly IViewPlacementService? _viewPlacementService;

    [ObservableProperty]
    private bool _preventLeave;

    public event AsyncEventHandler<DialogCloseEventArgs>? RequestCloseAsync;

    public string Title => $"{nameof(LightViewModel)}:{InstanceNumber}";

    public IconDescriptor Icon => IconDescriptor.FromFile("Icon.png");

    public LightViewModel(IRegionManager regionManager, IViewPlacementService? viewPlacementService = null)
    {
        _regionManager = regionManager;
        _viewPlacementService = viewPlacementService;
    }


    [RelayCommand]
    private Task UnloadView(string param)
    {
        return UnloadOrCloseFloatingAsync(_viewPlacementService);
    }

    [RelayCommand]
    private Task FloatView(string param)
    {
        return FloatOrDockAsync(_viewPlacementService);
    }

    public Task<bool> CanNavigateAsync(NavigationContext context, CancellationToken cancellationToken) =>
        Task.FromResult(!PreventLeave);

    [RelayCommand]
    private Task CloseDialog(string param)
    {
        return RequestCloseAsync!.Invoke(this, 
            new DialogCloseEventArgs(new DialogResult(DialogButtonResult.OK), 
            CancellationToken.None));
    }
    [RelayCommand]
    private Task CloseDialogWithCancelling(string param)
    {
        var cts = new CancellationTokenSource();
        cts.CancelAfter(TimeSpan.FromSeconds(2));
        return RequestCloseAsync!.Invoke(this, new DialogCloseEventArgs(new DialogResult(DialogButtonResult.OK), cts.Token));
    }
    [RelayCommand]
    private async Task AsyncPageNavigate(string param)
    {
        var (viewName, parameters) = SampleHelper.ParseNavigationParam(param);
        var ret = await _regionManager.RequestNavigateAsync("NavigationPageRegion", viewName);
    }
    [RelayCommand]
    private async Task AsyncTabbedPageNavigate(string param)
    {
        var (viewName, parameters) = SampleHelper.ParseNavigationParam(param);
        var ret = await _regionManager.RequestNavigateAsync("TabbedPageRegion", viewName);
    }
    public async Task OnDialogOpenedAsync(IDialogParameters? parameters, CancellationToken cancellationToken)
    {
        IsDialog = true;
        if (cancellationToken.CanBeCanceled)
        {
            await Task.Delay(TimeSpan.FromSeconds(2), cancellationToken);
        }
    }

    public async Task OnDialogClosingAsync(IDialogResult? dialogResult, CancellationToken cancellationToken)
    {
        if (cancellationToken.CanBeCanceled)
        {
            await Task.Delay(TimeSpan.FromSeconds(2), cancellationToken);
        }
    }

    public Task OnDialogClosedAsync(IDialogResult? dialogResult, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
