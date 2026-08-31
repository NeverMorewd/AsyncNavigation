using AsyncNavigation;
using AsyncNavigation.Abstractions;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Sample.Common;

public partial class InfinityViewModel : InstanceCounterViewModel<InfinityViewModel>
{
    [ObservableProperty]
    private string _nextRegionName = "";
    [ObservableProperty]
    private string _buttonText = "Next";
    [ObservableProperty]
    private bool _isActive = true;
    private readonly IRegionManager _regionManager;

    public InfinityViewModel(IRegionManager regionManager)
    {
        NextRegionName = $"InfinityRegion-{InstanceNumber}";
        _regionManager = regionManager;
    }

    [RelayCommand]
    private async Task AsyncNavigate(string param)
    {
        await _regionManager.RequestNavigateAsync(NextRegionName, param);
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
