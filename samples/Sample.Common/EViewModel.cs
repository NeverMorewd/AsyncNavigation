using ReactiveUI.SourceGenerators;
namespace Sample.Common;

public partial class EViewModel : InstanceCounterViewModel<EViewModel>
{
    [ReactiveCommand]
    private Task UnloadView(string param)
    {
        return RequestUnload();
    }

    [ReactiveCommand]
    private Task CloseDialog(string param)
    {
        return Task.CompletedTask;
    }
}
