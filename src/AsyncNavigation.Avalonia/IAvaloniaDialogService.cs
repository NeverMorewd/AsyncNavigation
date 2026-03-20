using AsyncNavigation.Abstractions;
using AsyncNavigation.Core;

namespace AsyncNavigation.Avalonia;

/// <summary>
/// Avalonia-specific dialog service that extends <see cref="IDialogService"/> with
/// the <c>FrontShowAsync</c> pattern for showing a dialog (e.g. splash screen or
/// login window) before the main application window is displayed.
/// </summary>
/// <remarks>
/// Register and resolve this interface instead of <see cref="IDialogService"/> in
/// Avalonia applications when you need the front-show functionality at startup.
/// The underlying <see cref="AsyncNavigation.DialogService"/> implements both
/// interfaces, so resolving <see cref="IAvaloniaDialogService"/> will return the
/// same service instance.
/// </remarks>
public interface IAvaloniaDialogService : IDialogService
{
    /// <summary>
    /// Displays a dialog view (e.g., a splash screen or login form hosted inside a
    /// container window) before showing the main application window.
    /// </summary>
    /// <typeparam name="TWindow">The main window type to create after the dialog closes.</typeparam>
    /// <param name="viewName">The registered name of the dialog view content.</param>
    /// <param name="mainWindowBuilder">
    /// A factory that receives the dialog result and returns the main window instance,
    /// or <see langword="null"/> to suppress main window creation.
    /// </param>
    /// <param name="containerName">Optional container window name; uses the default container if omitted.</param>
    /// <param name="parameters">Optional parameters passed to the dialog view model.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    new Task FrontShowViewAsync<TWindow>(
        string viewName,
        Func<IDialogResult, TWindow?> mainWindowBuilder,
        string? containerName = null,
        IDialogParameters? parameters = null,
        CancellationToken cancellationToken = default) where TWindow : class;

    /// <summary>
    /// Displays a dialog window (e.g., a splash screen or login window) before showing
    /// the main application window.
    /// </summary>
    /// <typeparam name="TWindow">The main window type to create after the dialog closes.</typeparam>
    /// <param name="windowName">The registered name of the dialog window.</param>
    /// <param name="mainWindowBuilder">
    /// A factory that receives the dialog result and returns the main window instance,
    /// or <see langword="null"/> to suppress main window creation.
    /// </param>
    /// <param name="parameters">Optional parameters passed to the dialog window view model.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    new Task FrontShowWindowAsync<TWindow>(
        string windowName,
        Func<IDialogResult, TWindow?> mainWindowBuilder,
        IDialogParameters? parameters = null,
        CancellationToken cancellationToken = default) where TWindow : class;
}
