using AsyncNavigation.Abstractions;
using ReactiveUI;
using ReactiveUI.SourceGenerators;

namespace Sample.Common;

public partial class MainWindowViewModel : ViewModelBase
{
    private readonly IRegionManager _regionManager;
    public MainWindowViewModel(IRegionManager regionManager)
    {
        _regionManager = regionManager;      
    }
    [Reactive]
    private bool _isSplitViewPaneOpen = false;
    [ReactiveCommand]
    private async Task Navigate(string param)
    {
        var result = await _regionManager.RequestNavigate("MainRegion", param);
    }
}
