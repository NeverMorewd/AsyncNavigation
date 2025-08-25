using AsyncNavigation.Core;

namespace AsyncNavigation.Abstractions;

public interface IDialogAware
{
    string Title { get; }
    event AsyncEventHandler<DialogCloseEventArgs>? RequestCloseAsync;
    Task OnDialogOpenedAsync(IDialogParameters? parameters);
    Task OnDialogClosingAsync(IDialogResult? dialogResult);
    Task OnDialogClosedAsync(IDialogResult? dialogResult);
}
