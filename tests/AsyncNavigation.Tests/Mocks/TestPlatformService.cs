using AsyncNavigation.Abstractions;
using AsyncNavigation.Core;

namespace AsyncNavigation.Tests.Mocks;

internal class TestPlatformService : IPlatformService
{
    public void AttachClosing(IDialogWindowBase window, Action<object?, WindowClosingEventArgs> handler)
    {
        return;
    }

    public void Show(IDialogWindowBase baseWindow, bool isModal)
    {
        return;
    }

    public Task ShowAsync(IDialogWindowBase baseWindow, bool isModal)
    {
        return Task.CompletedTask;
    }

    public void ShowMainWindow(object mainWindow)
    {
        return;
    }

    public T WaitOnDispatcher<T>(Task<T> task)
    {
        return task.Result;
    }

    public void WaitOnDispatcher(Task task)
    {
        task.Wait();
    }
}
