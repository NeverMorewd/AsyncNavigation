using AsyncNavigation;
using AsyncNavigation.Abstractions;
using AsyncNavigation.Core;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Sample.Common;

public partial class MainWindowViewModel : ViewModelBase
{
    private readonly IRegionManager _regionManager;
    private readonly IDialogService _dialogService;
    private readonly IRegistrationTracker _registrationTracker;
    private readonly IRouter? _router;
    
    public MainWindowViewModel(IRegionManager regionManager, 
        IDialogService dialogService,
        IRegistrationTracker registrationTracker,
        IRouter? router = null)
    {
        _router = router;
        _regionManager = regionManager;
        _dialogService = dialogService;
        _registrationTracker = registrationTracker;
        _regionManager
            .RequestNavigateAsync("MainRegion", "LightView", replay: false)
            .ContinueWith(t => 
            {
                if (t.IsFaulted)
                {
                    Debug.WriteLine($"RequestNavigate Failed:{t.Result.Exception}");
                }
            });

        if(_regionManager.TryGetRegion("MainRegion", out var mainRegion))
        {
            mainRegion.Navigated += (s, e) => 
            {
                Debug.WriteLine($"Navigated:{e.Context}");
            };
        }
        Views = _registrationTracker.TryGetViews(out var views) ? [.. views] : [];

        if (_router is not null)
        {
            foreach (var mappedNavigation in _router.Routes)
            {
                Views.Add(mappedNavigation.Path);
            }
            Views.Add("/Tab/Tab_A");
        }

    }

    public string FooterText => $"Powered by .NET {Environment.Version} • {RuntimeInformation.OSDescription}";

    [ObservableProperty]
    private bool _isSplitViewPaneOpen = true;

    public ObservableCollection<string> Views { get; }
    [ObservableProperty]
    private string? _selectedView;

    partial void OnSelectedViewChanged(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return;

        // Paths use router navigation; all other values are registered view names.
        if (value.StartsWith("/", StringComparison.OrdinalIgnoreCase))
            AsyncPathNavigateAndForget(value);
        else
            AsyncNavigateAndForget(value);
    }

    private bool CanUseDialogs() => !OperatingSystem.IsBrowser();

    public static bool FilterPredicate(string? search, object? item)
    {
        if (item is not null && !string.IsNullOrEmpty(search))
        {
            return item.ToString()!.Contains(search ?? string.Empty, StringComparison.OrdinalIgnoreCase);
        }
        return false;
    }
        

    [RelayCommand]
    private async Task AsyncNavigate(string param)
    {
        var (viewName, parameters) = SampleHelper.ParseNavigationParam(param);
        var result = await _regionManager.RequestNavigateAsync("MainRegion", viewName, parameters);
        Debug.WriteLine(result.Duration.TotalMilliseconds);
    }

    [RelayCommand]
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
    [RelayCommand(CanExecute = nameof(CanUseDialogs))]
    private void Show(string param)
    {
        _dialogService.ShowView(param, callBack: result => 
        {
            Debug.WriteLine(result.Result);
        });
    }

    [RelayCommand(CanExecute = nameof(CanUseDialogs))]
    private async Task AsyncShowDialog(string param)
    {
       var result = await _dialogService.ShowViewDialogAsync(param);
    }
    [RelayCommand(CanExecute = nameof(CanUseDialogs))]
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
    [RelayCommand(CanExecute = nameof(CanUseDialogs))]
    private void ShowDialog(string param)
    {
        var result = _dialogService.ShowViewDialog(param);
    }
    [RelayCommand(CanExecute = nameof(CanUseDialogs))]
    private void ShowWindow(string param)
    {
        var result = _dialogService.ShowWindowDialog(param);
    }

    [RelayCommand(CanExecute = nameof(CanUseDialogs))]
    private async Task AsyncShowWindow(string param)
    {
        await _dialogService.ShowWindowDialogAsync(param);
    }

    [RelayCommand]
    private async Task GoForward()
    {
        await _regionManager.GoForwardAsync("MainRegion");
    }

    [RelayCommand]
    private async Task GoBack()
    {
        await _regionManager.GoBackAsync("MainRegion");
    }

    [RelayCommand]
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
