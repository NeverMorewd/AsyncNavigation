namespace AsyncNavigation.Abstractions;


internal interface IPlatformService<TWindow> : IPlatformService
    where TWindow : class
{
    Task ShowAsync(TWindow window, bool isModal);
    void Show(TWindow window, bool isModal);
}
