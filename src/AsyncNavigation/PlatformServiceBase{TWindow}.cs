using AsyncNavigation.Abstractions;
using AsyncNavigation.Core;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace AsyncNavigation;


internal abstract class PlatformServiceBase<TWindow> : IPlatformService<TWindow>
    where TWindow : class
{
    public abstract Task ShowAsync(TWindow window, bool isModal);
    public abstract void Show(TWindow window, bool isModal);

    public Task ShowAsync(IWindowBase dialogWindow, bool isModal)
    {
        if(!TryGetPlatformWindow(dialogWindow, out TWindow? window))
        {
            throw new InvalidOperationException($"Window must be of type {typeof(TWindow).Name}, " +
                $"but got {dialogWindow?.GetType().Name ?? "null"}");
        }
        return ShowAsync(window, isModal);
    }
    public Task<IDialogResult> HandleDialogCloseAsync(IWindowBase dialogWindow, IDialogAware dialogAware)
    {
        return HandleCloseInternalAsync(dialogWindow, dialogAware);
    }

    public void Show(IWindowBase baseWindow, bool isModal)
    {
        if (!TryGetPlatformWindow(baseWindow, out TWindow? window))
        {
            throw new InvalidOperationException($"Window must be of type {typeof(TWindow).Name}, " +
                $"but got {window?.GetType().Name ?? "null"}");
        }
        Show(window, isModal);
    }
    protected Task<IDialogResult> HandleCloseInternalAsync(IWindowBase baseWindow, IDialogAware dialogAware)
    {
        ArgumentNullException.ThrowIfNull(baseWindow);
        ArgumentNullException.ThrowIfNull(dialogAware);

        var completionSource = new TaskCompletionSource<IDialogResult>();

        async Task RequestCloseHandler(object? sender, DialogCloseEventArgs args)
        {
            try
            {
                await dialogAware.OnDialogClosingAsync(args.DialogResult, args.CancellationToken);
                args.CancellationToken.ThrowIfCancellationRequested();

                completionSource.TrySetResult(args.DialogResult);
                baseWindow.Close();
                try
                {
                    await dialogAware.OnDialogClosedAsync(args.DialogResult, args.CancellationToken);
                }
                catch (OperationCanceledException) when (args.CancellationToken.IsCancellationRequested)
                {
                    Debug.WriteLine("OnDialogClosedAsync was cancelled, but dialog is already closed.");
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error in OnDialogClosedAsync: {ex}");
                }
            }
            catch (OperationCanceledException) when (args.CancellationToken.IsCancellationRequested)
            {
                var setCanceledFlag = completionSource.TrySetCanceled();
                Debug.WriteLineIf(!setCanceledFlag, "Failed to set canceled for DialogResult.");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in OnDialogClosingAsync: {ex}");

                if (!completionSource.Task.IsCompleted)
                {
                    var faultedResult = new DialogResult(DialogButtonResult.None);
                    completionSource.TrySetResult(faultedResult);
                }
            }
        }

        void ClosedHandler(object? sender, EventArgs e)
        {
            baseWindow.Closed -= ClosedHandler;
            dialogAware.RequestCloseAsync -= RequestCloseHandler;

            if (!completionSource.Task.IsCompleted)
            {
                completionSource.TrySetResult(new DialogResult(DialogButtonResult.None));
            }
        }

        dialogAware.RequestCloseAsync += RequestCloseHandler;
        baseWindow.Closed += ClosedHandler;

        return completionSource.Task;
    }
    private static bool TryGetPlatformWindow(IWindowBase baseWindow, [MaybeNullWhen(false)] out TWindow window)
    {
        return (window = baseWindow as TWindow) is not null;
    }

    public abstract T WaitOnDispatcher<T>(Task<T> task);

    public abstract void WaitOnDispatcher(Task task);
}
