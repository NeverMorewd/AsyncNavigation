using AsyncNavigation;
using AsyncNavigation.Abstractions;
using ReactiveUI.SourceGenerators;

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
    }
    [ReactiveCommand]
    private void AsyncNavigateAndForget(string param)
    {
        var (viewName, parameters) = SampleHelper.ParseNavigationParam(param);
        _ = _regionManager.RequestNavigateAsync("ItemsRegion", viewName, parameters).ContinueWith(t => 
        {
            var r = t.Result;
        });
    }
    [ReactiveCommand]
    private Task UnloadView(string param)
    {
        return RequestUnload();
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
