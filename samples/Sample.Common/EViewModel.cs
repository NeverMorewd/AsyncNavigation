using AsyncNavigation;
using AsyncNavigation.Abstractions;
using AsyncNavigation.Core;
using ReactiveUI.SourceGenerators;
using System.Collections.ObjectModel;

namespace Sample.Common;

public partial class EViewModel : InstanceCounterViewModel<EViewModel>, IDialogAware
{
    public ObservableCollection<byte> HeavyItems
    {
        get;
    } = [];

    public string Title => $"{nameof(EViewModel)}:{InstanceNumber}";

    public event AsyncEventHandler<DialogCloseEventArgs>? RequestCloseAsync;

    [ReactiveCommand]
    private Task UnloadView(string param)
    {
        return RequestUnloadAsync();
    }

    [ReactiveCommand]
    private Task CloseDialog(string param)
    {
        return RequestCloseAsync!.Invoke(this, 
            new DialogCloseEventArgs(new DialogResult(DialogButtonResult.OK), CancellationToken.None));
    }
    public override Task InitializeAsync(NavigationContext context)
    {
        AddItems(20);
        return base.InitializeAsync(context);
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

    private void AddItems(int count)
    {
        var rnd = new Random();
        for (int i = 0; i < count; i++)
        {
            HeavyItems.Add((byte)rnd.Next(0, 256));
        }
    }
}
