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
    private Task UnloadView(string param)
    {
        return RequestUnloadAsync(CancellationToken.None);
    }

    [ReactiveCommand]
    private Task CloseDialog(string param)
    {
        return RequestCloseAsync!.Invoke(this, 
            new DialogCloseEventArgs(new DialogResult(DialogButtonResult.OK), 
            CancellationToken.None));
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
        if (cancellationToken.CanBeCanceled)
        {
            await Task.Delay(TimeSpan.FromSeconds(2), cancellationToken);
        }
    }

    public async Task OnDialogClosingAsync(IDialogResult? dialogResult, CancellationToken cancellationToken)
    {
        if (cancellationToken.CanBeCanceled)
        {
            await Task.Delay(TimeSpan.FromSeconds(2), cancellationToken);
        }
    }

    public Task OnDialogClosedAsync(IDialogResult? dialogResult, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
