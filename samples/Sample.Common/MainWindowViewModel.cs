using AsyncNavigation;
using AsyncNavigation.Abstractions;
using AsyncNavigation.Core;
using DynamicData.Binding;
using ReactiveUI;
using ReactiveUI.SourceGenerators;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Reactive.Linq;

namespace Sample.Common;

public partial class MainWindowViewModel : ViewModelBase
{
    private readonly IRegionManager _regionManager;
    private readonly IDialogService _dialogService;
    private readonly IRegistrationTracker _registrationTracker;
    public MainWindowViewModel(IRegionManager regionManager, 
        IDialogService dialogService,
        IRegistrationTracker registrationTracker)
    {
        _regionManager = regionManager;
        _dialogService = dialogService;
        _registrationTracker = registrationTracker;
        _regionManager.RequestNavigateAsync("MainRegion", "AView", replay: false).ContinueWith(t => 
        {
            if (t.IsFaulted)
            {
                Debug.WriteLine($"RequestNavigate Failed:{t.Result.Exception}");
            }
        });
        Views = _registrationTracker.TryGetViews(out var views) ? [.. views] : [];

        this.WhenAnyValue(vm => vm.SelectedView)
            .Subscribe(viewName =>
            {
                if (!string.IsNullOrEmpty(viewName))
                {
                    AsyncNavigateAndForget(viewName);
                }
            });
    }
    [Reactive]
    private bool _isSplitViewPaneOpen = false;
    
    public ObservableCollection<string> Views { get; }
    [Reactive]
    private string? _selectedView;

    public static bool FilterPredicate(string? search, object? item)
    {
        if (item is not null && !string.IsNullOrEmpty(search))
        {
            return item.ToString()!.Contains(search ?? string.Empty, StringComparison.OrdinalIgnoreCase);
        }
        return false;
    }
        

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
        _dialogService.ShowView(param, callBack: result => 
        {
            Debug.WriteLine(result.Result);
        });
    }

    [ReactiveCommand]
    private async Task AsyncShowDialog(string param)
    {
       var result = await _dialogService.ShowViewDialogAsync(param);
    }
    [ReactiveCommand]
    private async Task AsyncShowDialogWithCancelling(string param)
    {
        var cts = new CancellationTokenSource();
        cts.CancelAfter(TimeSpan.FromSeconds(2));

        var result = await _dialogService.ShowViewDialogAsync(param, cancellationToken: cts.Token);

        if(result.Status == DialogStatus.Cancelled)
        {
            Debug.WriteLine("Dialog was cancelled");
        }
    }
    [ReactiveCommand]
    private void ShowDialog(string param)
    {
        var result = _dialogService.ShowViewDialog(param);
    }
    [ReactiveCommand]
    private void ShowWindow(string param)
    {
        var result = _dialogService.ShowWindowDialog(param);
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
