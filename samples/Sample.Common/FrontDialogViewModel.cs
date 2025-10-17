using AsyncNavigation.Abstractions;
using AsyncNavigation.Core;
using ReactiveUI.SourceGenerators;

namespace Sample.Common;
public partial class FrontDialogViewModel : InstanceCounterViewModel<FrontDialogViewModel>, IDialogAware
{
    [Reactive]
    private int _ratio;
    private CancellationTokenSource? _cts;
    public event AsyncEventHandler<DialogCloseEventArgs>? RequestCloseAsync;
    public string Title => $"{nameof(AViewModel)}:{InstanceNumber}";

    [ReactiveCommand]
    private Task CloseDialog(string param)
    {
        if (RequestCloseAsync != null)
        {
            var buttonResult = Ratio == 100 ? DialogButtonResult.Done : DialogButtonResult.Cancel;
            return RequestCloseAsync.Invoke(
                this,
                new DialogCloseEventArgs(
                    new DialogResult(buttonResult),
                    CancellationToken.None
                )
            );
        }
        return Task.CompletedTask;
    }

    public Task OnDialogOpenedAsync(IDialogParameters? parameters, CancellationToken cancellationToken)
    {
        IsDialog = true;

        _cts = new CancellationTokenSource();
        _ = StartProgressAsync(_cts.Token);

        return Task.CompletedTask;
    }

    public Task OnDialogClosingAsync(IDialogResult? dialogResult, CancellationToken cancellationToken)
    {
        if (Ratio != 100)
        {
            _cts?.Cancel();
        }
        _cts?.Dispose();
        _cts = null;
        //await Task.Delay(TimeSpan.FromSeconds(2));
        return Task.CompletedTask;
    }

    public Task OnDialogClosedAsync(IDialogResult? dialogResult, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    private async Task StartProgressAsync(CancellationToken token)
    {
        Ratio = 0;

        try
        {
            while (Ratio < 100 && !token.IsCancellationRequested)
            {
                Ratio++;
                await Task.Delay(20, token);
            }

            if (Ratio >= 100 && RequestCloseAsync != null && !token.IsCancellationRequested)
            {
                await RequestCloseAsync.Invoke(
                    this,
                    new DialogCloseEventArgs(
                        new DialogResult(DialogButtonResult.Done),
                        CancellationToken.None
                    )
                );
            }
        }
        catch (OperationCanceledException)
        {

        }
    }
}
