using AsyncNavigation;
using AsyncNavigation.Abstractions;
using ReactiveUI;
using ReactiveUI.SourceGenerators;
using System.Diagnostics;

namespace Sample.Common;

public partial class BViewModel : InstanceCounterViewModel<BViewModel>
{
    private readonly IRegionManager _regionManager;
    public BViewModel(IRegionManager regionManager)
    {
        _regionManager = regionManager;
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
        return RequestUnloadAsync();
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
