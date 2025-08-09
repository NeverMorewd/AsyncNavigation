using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AsyncNavigation.Abstractions;
using ReactiveUI.SourceGenerators;

namespace Sample.Common;

public partial class AViewModel : ViewModelBase
{
    private readonly IRegionManager _regionManager;
    public AViewModel(IRegionManager regionManager)
    {
        _regionManager = regionManager;
    }

    [ReactiveCommand]
    private async Task Navigate(string param)
    {
        var result = await _regionManager.RequestNavigate("ItemsRegion", param);
    }
}
