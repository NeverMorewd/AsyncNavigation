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
    /// Displays a dialog view before showing the main window.
    /// </summary>
    /// <remarks>
    /// For Avalonia applications, prefer resolving <c>IAvaloniaDialogService</c> and calling
    /// <c>FrontShowViewAsync</c> which provides a friendlier API with optional parameters.
    /// </remarks>
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
    /// Displays a dialog window before showing the main window.
    /// </summary>
    /// <remarks>
    /// For Avalonia applications, prefer resolving <c>IAvaloniaDialogService</c> and calling
    /// <c>FrontShowWindowAsync</c> which provides a friendlier API with optional parameters.
    /// </remarks>
    Task FrontShowAsync<TWindow>(string windowName,
      Func<IDialogResult, TWindow?> mainWindowBuilder,
      IDialogParameters? parameters,
      CancellationToken cancellationToken) where TWindow : class;
}
