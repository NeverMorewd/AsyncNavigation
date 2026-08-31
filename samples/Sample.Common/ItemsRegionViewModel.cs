using AsyncNavigation;
using AsyncNavigation.Abstractions;
using CommunityToolkit.Mvvm.Input;
using System.Diagnostics;

namespace Sample.Common;

public partial class ItemsRegionViewModel : InstanceCounterViewModel<ItemsRegionViewModel>
{
    private readonly IRegionManager _regionManager;
    public ItemsRegionViewModel(IRegionManager regionManager)
    {
        _regionManager = regionManager;
    }
    [RelayCommand]
    private async Task AsyncNavigate(string param)
    {
        var (viewName, parameters) = SampleHelper.ParseNavigationParam(param);
        var ret = await _regionManager.RequestNavigateAsync("ItemsRegion", viewName, parameters);
        Debug.WriteLine(ret);
    }
    [RelayCommand]
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
    [RelayCommand]
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
