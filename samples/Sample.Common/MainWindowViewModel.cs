using AsyncNavigation;
using AsyncNavigation.Abstractions;
using AsyncNavigation.Core;
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
    private readonly IRouter _router;
    public MainWindowViewModel(IRegionManager regionManager, 
        IDialogService dialogService,
        IRegistrationTracker registrationTracker,
        IRouter router)
    {
        _router = router;
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
        foreach (var mappedNavigation in _router.Routes)
        {
            Views.Add(mappedNavigation.Path);
        }
        Views.Add("/Tab/Tab_A");

        this.WhenAnyValue(vm => vm.SelectedView)
            .WhereNotNull()
            .Subscribe(target =>
            {
                if (target.StartsWith("/",StringComparison.OrdinalIgnoreCase))
                {
                    AsyncPathNavigateAndForget(target);
                }
                else
                {
                    AsyncNavigateAndForget(target);
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
    private void AsyncPathNavigateAndForget(string path)
    {
        _ = _regionManager.RequestPathNavigateAsync(path).ContinueWith(t =>
        {
            var result = t.Result;
            Debug.WriteLine($"RequestPathNavigateAsync:{result.Duration.TotalMilliseconds}");
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
        var beforeCollect = GC.GetTotalMemory(false);
        GC.Collect();
        GC.WaitForPendingFinalizers();
        var afterCollect = GC.GetTotalMemory(false);
        var freedMemory = beforeCollect - afterCollect;
        
        Debug.WriteLine($"release size: {FormatBytes(freedMemory)}");
        
    }
    
    private static string FormatBytes(long bytes)
    {
        string[] suffixes = ["B", "KB", "MB", "GB"];
        var counter = 0;
        decimal number = bytes;
    
        while (Math.Round(number / 1024) >= 1)
        {
            number /= 1024;
            counter++;
        }
    
        return $"{number:n2} {suffixes[counter]}";
    }
}
