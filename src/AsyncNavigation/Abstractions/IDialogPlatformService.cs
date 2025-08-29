namespace AsyncNavigation.Abstractions;

public interface IDialogPlatformService
{
    Task ShowAsync(IDialogWindowBase dialogWindow, bool isModal, object? owner = null);
    Task<IDialogResult> HandleCloseAsync(IDialogWindowBase dialogWindow, IDialogAware dialogAware);
    IDialogResult WaitOnUIThread(Task<IDialogResult> dialogResultTask);
    void WaitOnUIThread(Task showDialogTask);
}
