namespace AsyncNavigation.Abstractions;


internal interface IPlatformService<TWindow> : IPlatformService
    where TWindow : class
{
    Task ShowAsync(TWindow window, bool isModal);
    void Show(TWindow window, bool isModal);
    void AttachClosing(TWindow window, Action<object?, Core.WindowClosingEventArgs> handler);
    void ShowMainWindow(TWindow mainWindow);
}
