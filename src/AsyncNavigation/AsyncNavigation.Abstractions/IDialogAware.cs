using AsyncNavigation.Core;

namespace AsyncNavigation.Abstractions;

public interface IDialogAware
{
    string Title { get; }
    event Action<IDialogResult>? RequestClose;
    Task OnDialogClosedAsync();
    Task OnDialogClosedAsync(IDialogParameters? parameters);
}
