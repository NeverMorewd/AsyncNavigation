using AsyncNavigation.Abstractions;
using AsyncNavigation.Core;
using ReactiveUI.SourceGenerators;

namespace Sample.Common;

public partial class EViewModel : InstanceCounterViewModel<EViewModel>, IDialogAware
{
    public string Title => $"{nameof(EViewModel)}:{InstanceNumber}";

    public event AsyncEventHandler<DialogCloseEventArgs>? RequestCloseAsync;

    [ReactiveCommand]
    private Task UnloadView(string param)
    {
        return RequestUnload();
    }

    [ReactiveCommand]
    private Task CloseDialog(string param)
    {
        return RequestCloseAsync!.Invoke(this, 
            new DialogCloseEventArgs(new DialogResult(DialogButtonResult.OK), CancellationToken.None));
    }

    public async Task OnDialogOpenedAsync(IDialogParameters? parameters, CancellationToken cancellationToken)
    {
        IsDialog = true;
        if (cancellationToken != CancellationToken.None)
        {
            await Task.Delay(TimeSpan.FromSeconds(5), cancellationToken);
        }
    }

    public Task OnDialogClosingAsync(IDialogResult? dialogResult, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    public Task OnDialogClosedAsync(IDialogResult? dialogResult, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
