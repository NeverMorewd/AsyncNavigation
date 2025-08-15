using AsyncNavigation.Abstractions;
using AsyncNavigation.Core;
using ReactiveUI.SourceGenerators;

namespace Sample.Common;

public partial class BViewModel : ViewModelBase
{
    private readonly IRegionManager _regionManager;
    public BViewModel(IRegionManager regionManager)
    {
        _regionManager = regionManager;
    }

    [ReactiveCommand]
    private async Task AsyncNavigate(string param)
    {
        var (viewName, parameters) = CommonHelper.ParseNavigationParam(param);
        await _regionManager.RequestNavigate("ItemsRegion", viewName, parameters);
    }

    public override async Task OnNavigatedToAsync(NavigationContext context,  CancellationToken cancellationToken)
    {
        await base.OnNavigatedToAsync(context, cancellationToken);

        //simulate delay
        await Task.Delay(5000, cancellationToken);
    }
}
