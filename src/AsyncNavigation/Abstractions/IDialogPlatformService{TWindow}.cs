namespace AsyncNavigation.Abstractions;


internal interface IDialogPlatformService<TWindow> : IDialogPlatformService
    where TWindow : class
{
    bool IsPlatformWindow(IDialogWindowBase? dialogWindow);
    Task ShowAsync(TWindow window, bool isModal, TWindow? owner = null);
    Task<IDialogResult> HandleCloseAsync(TWindow dialogWindow, IDialogAware dialogAware);
}
