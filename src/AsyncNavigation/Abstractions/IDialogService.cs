namespace AsyncNavigation.Abstractions;

public interface IDialogService
{
    void Show(string name,
        string? containerName,
        IDialogParameters? parameters,
        Action<IDialogResult>? callback,
        CancellationToken cancellationToken);

    Task<IDialogResult> ShowDialogAsync(string name,
        string? containerName,
        IDialogParameters? parameters,
        CancellationToken cancellationToken);

    IDialogResult ShowDialog(string name,
        string? containerName,
        IDialogParameters? parameters,
        CancellationToken cancellationToken);

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
    /// <param name="containerName">Optional: The name of the window container.</param>
    /// <param name="parameters">Optional: The parameters passed to the dialog.</param>
    /// <param name="cancellationToken">Optional: Token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task FrontShowAsync<TWindow>(string name,
      Func<IDialogResult, TWindow?> mainWindowBuilder,
      string? containerName,
      IDialogParameters? parameters,
      CancellationToken cancellationToken) where TWindow : class;

    void Show(string windowName,
        IDialogParameters? parameters,
        Action<IDialogResult>? callback,
        CancellationToken cancellationToken);

    Task<IDialogResult> ShowDialogAsync(string windowName,
        IDialogParameters? parameters,
        CancellationToken cancellationToken);

    IDialogResult ShowDialog(string windowName,
        IDialogParameters? parameters,
        CancellationToken cancellationToken);

    /// <summary>
    /// Displays a dialog (e.g., splash screen or login window) before showing the main window.
    /// This method is particularly useful in AvaloniaUI applications, where an initial dialog
    /// may need to appear before the main window is created or displayed.
    /// </summary>
    /// <typeparam name="TWindow">The main window type to be created after the dialog is closed.</typeparam>
    /// <param name="windowName">The name of the dialog window to show.</param>
    /// <param name="mainWindowBuilder">
    /// A function that builds the main window based on the <see cref="IDialogResult"/> returned by the dialog.
    /// </param>
    /// <param name="parameters">Optional: The parameters passed to the dialog.</param>
    /// <param name="cancellationToken">Optional: Token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task FrontShowAsync<TWindow>(string windowName,
      Func<IDialogResult, TWindow?> mainWindowBuilder,
      IDialogParameters? parameters,
      CancellationToken cancellationToken) where TWindow : class;
}
