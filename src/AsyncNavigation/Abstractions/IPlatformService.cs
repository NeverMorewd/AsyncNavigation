namespace AsyncNavigation.Abstractions;

public interface IPlatformService : ITaskExentsionProvder
{
    Task ShowAsync(IWindowBase baseWindow, bool isModal);
    void Show(IWindowBase baseWindow, bool isModal);
}
