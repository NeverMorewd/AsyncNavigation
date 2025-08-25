using AsyncNavigation;
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
    private void ShowDialog(string param)
    {
        var result = _dialogService.ShowDialog(param);
    }

}
