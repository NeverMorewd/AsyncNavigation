using AsyncNavigation;
using AsyncNavigation.Abstractions;
using ReactiveUI;
using ReactiveUI.SourceGenerators;
using Sample.Common.Messages;
using System.Diagnostics;

namespace Sample.Common;

public partial class BViewModel : InstanceCounterViewModel<BViewModel>
{
    private readonly IRegionManager _regionManager;

    // Last notification received via IMessenger — shown in the view.
    [Reactive]
    private string _lastNotification = "—";

    public BViewModel(IRegionManager regionManager, IMessenger messenger)
    {
        _regionManager = regionManager;

        // Static-style subscription: the lambda is static (no closure over 'this'),
        // and the messenger holds only a WeakReference to the recipient — no memory
        // leak even if Unsubscribe is never called explicitly.
        messenger.Subscribe<BViewModel, ViewAttachedMessage>(this,
            static (self, msg) =>
            {
                var text = $"{msg.Source} attached at {msg.Timestamp:HH:mm:ss} ({msg.PlatformContextName})";
                self.LastNotification = text;
                Debug.WriteLine($"[{nameof(BViewModel)}] Received ViewAttachedMessage: {text}");
            });
    }

    [ReactiveCommand]
    private async Task AsyncNavigate(string param)
    {
        var (viewName, parameters) = SampleHelper.ParseNavigationParam(param);
        var ret = await _regionManager.RequestNavigateAsync("ItemsRegion", viewName, parameters);
        Debug.WriteLine(ret);
    }

    [ReactiveCommand]
    private Task AsyncNavigateAndForget(string param)
    {
        var (viewName, parameters) = SampleHelper.ParseNavigationParam(param);
        _ = _regionManager.RequestNavigateAsync("ItemsRegion", viewName, parameters).ContinueWith(t =>
        {
            var result = t.Result;
            Debug.WriteLine(result);
        });
        return Task.CompletedTask;
    }

    [ReactiveCommand]
    private Task UnloadView(string param)
    {
        return RequestUnloadAsync(CancellationToken.None);
    }

    public override async Task OnNavigatedToAsync(NavigationContext context)
    {
        await base.OnNavigatedToAsync(context);
    }

    public override async Task OnNavigatedFromAsync(NavigationContext context)
    {
        await base.OnNavigatedFromAsync(context);
    }
}
