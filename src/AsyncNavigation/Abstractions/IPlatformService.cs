namespace AsyncNavigation.Abstractions;

public interface IPlatformService : ITaskExentsionProvder
{
    Task ShowAsync(IDialogWindowBase baseWindow, bool isModal);
    void Show(IDialogWindowBase baseWindow, bool isModal);
    void AttachClosing(IDialogWindowBase window, Action<object?, Core.WindowClosingEventArgs> handler);
    void ShowMainWindow(object mainWindow);
}
