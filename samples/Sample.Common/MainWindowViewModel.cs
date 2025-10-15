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
    public MainWindowViewModel(IRegionManager regionManager, 
        IDialogService dialogService,
        IPlatformService platformService)
    {
        _regionManager = regionManager;
        _dialogService = dialogService;

        // In Avalonia, the application lifetime hasn't started yet at this point,
        // and regions registered in XAML are not yet initialized. Therefore, we need
        // to set replay:true to make this navigation request pending until the region
        // registration is completed. This is not a concern in WPF.
        _regionManager.RequestNavigateAsync("MainRegion", "AView", replay: true).ContinueWith(t => 
        {
            if (t.IsFaulted)
            {
                Debug.WriteLine($"RequestNavigate Failed:{t.Result.Exception}");
            }
        });
        platformService.WaitOnDispatcher(Task.Delay(100));
    }
    [Reactive]
    private bool _isSplitViewPaneOpen = false;

    [ReactiveCommand]
    private async Task AsyncNavigate(string param)
    {
        var (viewName, parameters) = SampleHelper.ParseNavigationParam(param);
        var result = await _regionManager.RequestNavigateAsync("MainRegion", viewName, parameters);
        Debug.WriteLine(result.Duration.TotalMilliseconds);
    }

    [ReactiveCommand]
    private void AsyncNavigateAndForget(string param)
    {
        var (viewName, parameters) = SampleHelper.ParseNavigationParam(param);
        _ = _regionManager.RequestNavigateAsync("MainRegion", viewName, parameters).ContinueWith(t => 
        {
            var result = t.Result;
            Debug.WriteLine(result.Duration.TotalMilliseconds);
        });
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

        if(result.Status == DialogStatus.Cancelled)
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

    [ReactiveCommand]
    private void Collect()
    {
        GC.Collect();
        GC.WaitForPendingFinalizers();
    }
}
