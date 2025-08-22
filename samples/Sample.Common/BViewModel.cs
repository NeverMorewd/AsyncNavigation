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

    public override async Task OnNavigatedToAsync(NavigationContext context, CancellationToken cancellationToken)
    {
        await base.OnNavigatedToAsync(context, cancellationToken);

        //simulate delay
        await Task.Delay(2000, cancellationToken);
    }

    public override async Task OnNavigatedFromAsync(NavigationContext context, CancellationToken cancellationToken)
    {
        await base.OnNavigatedFromAsync(context, cancellationToken);

        //simulate delay
        await Task.Delay(2000, cancellationToken);
    }
}
