namespace AsyncNavigation.Abstractions;

public interface IDialogService
{
    void Show(string name,
        string? windowName = null,
        IDialogParameters? parameters = null,
        object? owner = null,
        Action<IDialogResult>? callback = null);

    Task<IDialogResult> ShowDialogAsync(string name,
        string? windowName = null,
        IDialogParameters? parameters = null,
        object? owner = null);

    IDialogResult ShowDialog(string name,
        string? windowName = null,
        IDialogParameters? parameters = null,
        object? owner = null);
}
