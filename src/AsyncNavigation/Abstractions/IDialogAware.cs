using AsyncNavigation.Core;

namespace AsyncNavigation.Abstractions;

public interface IDialogAware
{
    string Title { get; }
    event AsyncEventHandler<DialogCloseEventArgs>? RequestCloseAsync;
    Task OnDialogOpenedAsync(IDialogParameters? parameters, CancellationToken cancellationToken);
    Task OnDialogClosingAsync(IDialogResult? dialogResult, CancellationToken cancellationToken);
    Task OnDialogClosedAsync(IDialogResult? dialogResult, CancellationToken cancellationToken);
}
