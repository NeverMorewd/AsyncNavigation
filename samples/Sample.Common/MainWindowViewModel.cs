using AsyncNavigation.Abstractions;
using AsyncNavigation.Core;
using ReactiveUI;
using ReactiveUI.SourceGenerators;
using System.Diagnostics;

namespace Sample.Common;

public partial class MainWindowViewModel : ViewModelBase
{
    private readonly IRegionManager _regionManager;
    private readonly IDialogService _dialogService;
    public MainWindowViewModel(IRegionManager regionManager, IDialogService dialogService)
    {
        _regionManager = regionManager;
        _dialogService = dialogService;
    }
    [Reactive]
    private bool _isSplitViewPaneOpen = false;

    [ReactiveCommand]
    private async Task AsyncNavigate(string param)
    {
        var (viewName, parameters) = SampleHelper.ParseNavigationParam(param);
        await _regionManager.RequestNavigate("MainRegion", viewName, parameters);
    }

    [ReactiveCommand]
    private void AsyncNavigateWithoutWait(string param)
    {
        var (viewName, parameters) = SampleHelper.ParseNavigationParam(param);
        _ = _regionManager.RequestNavigate("MainRegion", viewName, parameters);
    }

    [ReactiveCommand]
    private async Task AsyncDelayNavigate(string param)
    {
        var (viewName, parameters) = SampleHelper.ParseNavigationParam(param);
        parameters ??= new NavigationParameters();
        parameters!.Add("delay", TimeSpan.FromSeconds(1));
        await _regionManager.RequestNavigate("MainRegion", viewName, parameters);
    }

    [ReactiveCommand]
    private void Show(string param)
    {
        _dialogService.Show(param, callback: result => 
        {
            Debug.WriteLine(result.Result);
        });
    }

    [ReactiveCommand]
    private async Task AsyncShowDialog(string param)
    {
       var result = await _dialogService.ShowDialogAsync(param);
    }
    [ReactiveCommand]
    private async Task AsyncShowDialogWithCancelling(string param)
    {
        var cts = new CancellationTokenSource();
        cts.CancelAfter(TimeSpan.FromSeconds(2));

        var result = await _dialogService.ShowDialogAsync(param, cancellationToken: cts.Token);

        if(result.Status == DialogResultStatus.Cancelled)
        {
            Debug.WriteLine("Dialog was cancelled");
        }
    }
    [ReactiveCommand]
    private void ShowDialog(string param)
    {
        var result = _dialogService.ShowDialog(param);
    }

    [ReactiveCommand]
    private async Task GoForward()
    {
        await _regionManager.GoForward("MainRegion");
    }

    [ReactiveCommand]
    private async Task GoBack()
    {
        await _regionManager.GoBack("MainRegion");
    }
}
