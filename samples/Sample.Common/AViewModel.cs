using AsyncNavigation.Abstractions;
using AsyncNavigation.Core;
using ReactiveUI.SourceGenerators;

namespace Sample.Common;

public partial class AViewModel : InstanceCounterViewModel<AViewModel>, IDialogAware
{
    private readonly IRegionManager _regionManager;

    public event AsyncEventHandler<DialogCloseEventArgs>? RequestCloseAsync;

    public string Title => $"{nameof(AViewModel)}:{InstanceNumber}";

    public AViewModel(IRegionManager regionManager)
    {
        _regionManager = regionManager;
    }

    [ReactiveCommand]
    private async Task AsyncNavigate(string param)
    {
        var (viewName, parameters) = SampleHelper.ParseNavigationParam(param);
        await _regionManager.RequestNavigate("ItemsRegion", viewName, parameters);
    }

    [ReactiveCommand]
    private Task UnloadView(string param)
    {
        return RequestUnload();
    }

    [ReactiveCommand]
    private Task CloseDialog(string param)
    {
        return RequestCloseAsync!.Invoke(this, new DialogCloseEventArgs(new DialogResult(DialogButtonResult.OK), CancellationToken.None));
    }
    public Task OnDialogOpenedAsync(IDialogParameters? parameters)
    {
        IsDialog = true;
        return Task.CompletedTask;
    }

    public Task OnDialogClosingAsync(IDialogResult? dialogResult)
    {
        return Task.CompletedTask;
    }

    public Task OnDialogClosedAsync(IDialogResult? dialogResult)
    {
        return Task.CompletedTask;
    }
}
