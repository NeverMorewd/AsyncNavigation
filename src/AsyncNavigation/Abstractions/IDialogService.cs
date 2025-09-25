namespace AsyncNavigation.Abstractions;

public interface IDialogService
{
    void Show(string name,
        string? windowName = null,
        IDialogParameters? parameters = null,
        CancellationToken cancellationToken = default,
        Action<IDialogResult>? callback = null);

    Task<IDialogResult> ShowDialogAsync(string name,
        string? windowName = null,
        IDialogParameters? parameters = null,
        CancellationToken cancellationToken = default);

    IDialogResult ShowDialog(string name,
        string? windowName = null,
        IDialogParameters? parameters = null,
        CancellationToken cancellationToken = default);
}
