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
    [ReactiveCommand]
    private Task CloseDialogWithCancelling(string param)
    {
        var cts = new CancellationTokenSource();
        cts.CancelAfter(TimeSpan.FromSeconds(2));
        return RequestCloseAsync!.Invoke(this, new DialogCloseEventArgs(new DialogResult(DialogButtonResult.OK), cts.Token));
    }
    public async Task OnDialogOpenedAsync(IDialogParameters? parameters, CancellationToken cancellationToken)
    {
        IsDialog = true;
        if (cancellationToken != CancellationToken.None)
        {
            await Task.Delay(TimeSpan.FromSeconds(5), cancellationToken);
        }
    }

    public async Task OnDialogClosingAsync(IDialogResult? dialogResult, CancellationToken cancellationToken)
    {
        if (cancellationToken != CancellationToken.None)
        {
            await Task.Delay(TimeSpan.FromSeconds(5), cancellationToken);
        }
    }

    public Task OnDialogClosedAsync(IDialogResult? dialogResult, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
