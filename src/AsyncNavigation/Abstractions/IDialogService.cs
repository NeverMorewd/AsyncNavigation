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

    /// <summary>
    /// Displays a dialog (e.g., splash screen or login window) before showing the main window.
    /// This method is particularly useful in AvaloniaUI applications, where an initial dialog
    /// may need to appear before the main window is created or displayed.
    /// </summary>
    /// <typeparam name="TWindow">The main window type to be created after the dialog is closed.</typeparam>
    /// <param name="name">The name of the dialog view to show.</param>
    /// <param name="mainWindowBuilder">
    /// A function that builds the main window based on the <see cref="IDialogResult"/> returned by the dialog.
    /// </param>
    /// <param name="windowName">Optional: The name of the parent window container, if any.</param>
    /// <param name="parameters">Optional: The parameters passed to the dialog.</param>
    /// <param name="cancellationToken">Optional: Token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>

    Task FrontShowAsync<TWindow>(string name,
      Func<IDialogResult, TWindow> mainWindowBuilder,
      string? windowName = null,
      IDialogParameters? parameters = null,
      CancellationToken cancellationToken = default) where TWindow : class;



    void ShowWindow(string windowName,
        IDialogParameters? parameters = null,
        Action<IDialogResult>? callback = null,
        CancellationToken cancellationToken = default);

    Task<IDialogResult> ShowDialogWindowAsync(string windowName,
        IDialogParameters? parameters = null,
        CancellationToken cancellationToken = default);

    IDialogResult ShowDialogWindow(string windowName,
        IDialogParameters? parameters = null,
        CancellationToken cancellationToken = default);

    Task FrontShowWindowAsync<TWindow>(string windowName,
      Func<IDialogResult, TWindow> mainWindowBuilder,
      IDialogParameters? parameters = null,
      CancellationToken cancellationToken = default) where TWindow : class;
}
