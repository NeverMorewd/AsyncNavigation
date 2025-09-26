namespace AsyncNavigation.Abstractions;

public interface IDialogService
{
    void Show(string name,
        string? windowName = null,
        IDialogParameters? parameters = null,
        Action<IDialogResult>? callback = null,
        CancellationToken cancellationToken = default);

    Task<IDialogResult> ShowDialogAsync(string name,
        string? windowName = null,
        IDialogParameters? parameters = null,
        CancellationToken cancellationToken = default);

    IDialogResult ShowDialog(string name,
        string? windowName = null,
        IDialogParameters? parameters = null,
        CancellationToken cancellationToken = default);
}
