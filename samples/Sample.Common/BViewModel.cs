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
        await _regionManager.RequestNavigate("ItemsRegion", viewName, parameters);
    }

    public override async Task OnNavigatedToAsync(NavigationContext context)
    {
        await base.OnNavigatedToAsync(context);
        //simulate delay
        await Task.Delay(2000, context.CancellationToken);
    }

    public override async Task OnNavigatedFromAsync(NavigationContext context)
    {
        await base.OnNavigatedFromAsync(context);
        //simulate delay
        await Task.Delay(2000, context.CancellationToken);
    }
}
