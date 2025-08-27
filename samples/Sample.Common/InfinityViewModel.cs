using AsyncNavigation;
using AsyncNavigation.Abstractions;
using ReactiveUI.SourceGenerators;

namespace Sample.Common;

public partial class InfinityViewModel: InstanceCounterViewModel<AViewModel>
{
    [Reactive]
    private string _nextRegionName = "";
    [Reactive]
    private string _buttonText = "Next";
    [Reactive]
    private bool _isActive = true;
    private readonly IRegionManager _regionManager;

    public InfinityViewModel(IRegionManager regionManager)
    {
        NextRegionName = $"InfinityRegion-{InstanceNumber}";
        _regionManager = regionManager;
    }

    [ReactiveCommand]
    private async Task AsyncNavigate(string param)
    {
        await _regionManager.RequestNavigate(NextRegionName, param);
        IsActive = false;
        ButtonText = "Refresh";
    }

    public override Task<bool> IsNavigationTargetAsync(NavigationContext context)
    {
        return Task.FromResult(false);
    }

    public override Task OnNavigatedToAsync(NavigationContext context)
    {
        IsActive = true;
        return base.OnNavigatedToAsync(context);
    }

    public override Task OnNavigatedFromAsync(NavigationContext context)
    {
        IsActive = false;
        return base.OnNavigatedFromAsync(context);
    }
}
