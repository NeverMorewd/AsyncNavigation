using AsyncNavigation.Abstractions;
using AsyncNavigation.Core;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Threading;
using System.Diagnostics;

namespace AsyncNavigation.Avalonia;

internal class DialogPlatformService : IPlatformService<Window>
{
    public Task<IDialogResult> HandleCloseAsync(Window dialogWindow, IDialogAware dialogAware)
    {
        ArgumentNullException.ThrowIfNull(dialogWindow);
        ArgumentNullException.ThrowIfNull(dialogAware);

        var completionSource = new TaskCompletionSource<IDialogResult>();

        async Task RequestCloseHandler(object? sender, DialogCloseEventArgs args)
        {
            try
            {
                await dialogAware.OnDialogClosingAsync(args.DialogResult, args.CancellationToken);
                args.CancellationToken.ThrowIfCancellationRequested();

                completionSource.TrySetResult(args.DialogResult);
                dialogWindow.Close();

                try
                {
                    await dialogAware.OnDialogClosedAsync(args.DialogResult, args.CancellationToken);
                }
                catch (OperationCanceledException) when (args.CancellationToken.IsCancellationRequested)
                {
                    Debug.WriteLine("DialogPlatformService: OnDialogClosedAsync was cancelled, but dialog is already closed.");
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"DialogPlatformService: Error in OnDialogClosedAsync: {ex}");
                }
            }
            catch (OperationCanceledException) when (args.CancellationToken.IsCancellationRequested)
            {
                var setCanceledFlag = completionSource.TrySetCanceled();
                Debug.WriteLineIf(!setCanceledFlag, "DialogPlatformService: Failed to set canceled for DialogResult.");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"DialogPlatformService: Error in OnDialogClosingAsync: {ex}");

                if (!completionSource.Task.IsCompleted)
                {
                    var faultedResult = new DialogResult(DialogButtonResult.None);
                    completionSource.TrySetResult(faultedResult);
                }
            }
        }

        void ClosedHandler(object? sender, EventArgs e)
        {
            dialogWindow.Closed -= ClosedHandler;
            dialogAware.RequestCloseAsync -= RequestCloseHandler;

            if (!completionSource.Task.IsCompleted)
            {
                completionSource.TrySetResult(new DialogResult(DialogButtonResult.None));
            }
        }

        dialogAware.RequestCloseAsync += RequestCloseHandler;
        dialogWindow.Closed += ClosedHandler;

        return completionSource.Task;
    }

    Task<IDialogResult> IPlatformService.HandleDialogCloseAsync(IWindowBase dialogWindow, IDialogAware dialogAware)
    {
        if (!IsPlatformWindow(dialogWindow))
        {
            throw new InvalidOperationException($"Dialog window must be of type {typeof(Window).Name}, but got {dialogWindow?.GetType().Name ?? "null"}");
        }
        return HandleCloseAsync((Window)dialogWindow, dialogAware);
    }

    public async Task ShowAsync(Window dialogWindow, bool isModal, Window? owner = null)
    {
        ArgumentNullException.ThrowIfNull(dialogWindow);

        if (isModal)
        {
            if(owner == null)
            {
                if (Application.Current!.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktopLifetime)
                {
                    owner = desktopLifetime.Windows.LastOrDefault(w => w.IsActive);
                    owner ??= desktopLifetime.MainWindow;
                }
                else
                {
                    throw new NotSupportedException($"Lifetime: '{Application.Current!.ApplicationLifetime?.GetType()}' is not supported");
                }
            }
            if (owner != null)
            {
                await dialogWindow.ShowDialog(owner);
            }
        }
        else
        {
            dialogWindow.Show();
        }
    }

    Task IPlatformService.ShowAsync(IWindowBase dialogWindow, bool isModal, object? owner)
    {
        if (!IsPlatformWindow(dialogWindow))
        {
            throw new InvalidOperationException($"Dialog window must be of type {typeof(Window).Name}, but got {dialogWindow?.GetType().Name ?? "null"}");
        }

        Window? ownerWindow = null;
        if (owner != null)
        {
            ownerWindow = owner as Window ??
                throw new InvalidOperationException($"Owner must be of type {typeof(Window).Name}, but got {owner.GetType().Name}");
        }

        return ShowAsync((Window)dialogWindow, isModal, ownerWindow);
    }

    public bool IsPlatformWindow(IWindowBase? window)
    {
        return window is Window;
    }

    public IDialogResult WaitOnUIThread(Task<IDialogResult> dialogResultTask)
    {
        return dialogResultTask.WaitOnDispatcherFrame();
    }

    public void WaitOnUIThread(Task showDialogTask)
    {
        showDialogTask.WaitOnDispatcherFrame();
    }
}