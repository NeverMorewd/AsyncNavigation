namespace AsyncNavigation.Abstractions;

public interface IPlatformService : ITaskExtensionProvider
{
    Task ShowAsync(IDialogWindowBase baseWindow, bool isModal);
    void Show(IDialogWindowBase baseWindow, bool isModal);
    void AttachClosing(IDialogWindowBase window, Action<object?, Core.WindowClosingEventArgs> handler);
    void ShowMainWindow(object mainWindow);

    /// <summary>
    /// Creates a platform-specific <see cref="IViewContext"/> that wraps the dialog window.
    /// Called by <see cref="IDialogService"/> to supply <see cref="IViewAware"/> dialog ViewModels
    /// with access to platform services (StorageProvider, Clipboard, Window, Dispatcher, …).
    /// </summary>
    IViewContext CreateDialogViewContext(IDialogWindowBase dialogWindow);
}
