using AsyncNavigation.Abstractions;
using AsyncNavigation.Core;
using Microsoft.Extensions.DependencyInjection;

namespace AsyncNavigation;

public class DialogService : IDialogService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IPlatformService _platformService;

    public DialogService(IServiceProvider serviceProvider, IPlatformService platformService)
    {
        _serviceProvider = serviceProvider;
        _platformService = platformService;
    }

    public async Task<IDialogResult> ShowDialogAsync(string name,
        string? windowName = null,
        IDialogParameters? parameters = null,
        CancellationToken cancellationToken = default)
    {
        var (dialogWindow, aware) = PrepareDialog(name, windowName);

        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            await aware.OnDialogOpenedAsync(parameters, cancellationToken);
            cancellationToken.ThrowIfCancellationRequested();
        }
        catch (OperationCanceledException)
        {
            return DialogResult.Cancelled;
        }
        var closeTask = HandleCloseInternalAsync(dialogWindow, aware);
        await _platformService.ShowAsync(dialogWindow, true);
        return await closeTask;
    }
    public IDialogResult ShowDialog(string name, 
        string? windowName = null, 
        IDialogParameters? parameters = null, 
        CancellationToken cancellationToken = default)
    {
        var showTask = ShowDialogAsync(name, windowName, parameters, cancellationToken);
        return _platformService.WaitOnDispatcher(showTask);
    }
    public void Show(string name, 
        string? windowName = null, 
        IDialogParameters? parameters = null,  
        Action<IDialogResult>? callback = null,
        CancellationToken cancellationToken = default)
    {
        var (dialogWindow, aware) = PrepareDialog(name, windowName);

        var openTask = aware.OnDialogOpenedAsync(parameters, cancellationToken);
        _platformService.WaitOnDispatcher(openTask);

        var closeTask = HandleCloseInternalAsync(dialogWindow, aware);
        _ = closeTask.ContinueWith(t =>
        {
            if (t.Status == TaskStatus.RanToCompletion)
            {
                callback?.Invoke(t.Result);
            }
        }, cancellationToken);

        _platformService.Show(dialogWindow, false);
    }
    public async Task FrontShowAsync<TWindow>(string name,
        Func<IDialogResult, TWindow> mainWindowBuilder,
        string? windowName = null,
        IDialogParameters? parameters = null,
        CancellationToken cancellationToken = default) where TWindow:class
    {
        var (dialogWindow, aware) = PrepareDialog(name, windowName);

        var openTask = aware.OnDialogOpenedAsync(parameters, cancellationToken);
        await openTask;

        var closeTask = HandleCloseInternalAsync(dialogWindow, aware, mainWindowBuilder);

        _platformService.ShowMainWindow(dialogWindow);
        _platformService.Show(dialogWindow, false);
        await closeTask;
    }
    protected Task<IDialogResult> HandleCloseInternalAsyncOld(IDialogWindowBase baseWindow, 
        IDialogAware dialogAware)
    {
        var tcs = new TaskCompletionSource<IDialogResult>();
        IDialogResult? pendingResult = null;

        async Task RequestCloseHandler(object? sender, DialogCloseEventArgs args)
        {
            try
            {
                await dialogAware.OnDialogClosingAsync(args.DialogResult, args.CancellationToken);
                args.CancellationToken.ThrowIfCancellationRequested();

                pendingResult = args.DialogResult;
                baseWindow.Close();
            }
            catch (OperationCanceledException)
            {
                pendingResult = DialogResult.Cancelled;
            }
        }

        void ClosedHandler(object? sender, EventArgs e)
        {
            baseWindow.Closed -= ClosedHandler;
            dialogAware.RequestCloseAsync -= RequestCloseHandler;
            tcs.TrySetResult(pendingResult ?? new DialogResult(DialogButtonResult.None));
        }

        dialogAware.RequestCloseAsync += RequestCloseHandler;
        baseWindow.Closed += ClosedHandler;

        return tcs.Task;
    }

    private IDialogWindow ResolveDialogWindow(string? windowName) =>
        string.IsNullOrEmpty(windowName)
            ? _serviceProvider.GetRequiredKeyedService<IDialogWindow>(NavigationConstants.DEFAULT_DIALOG_WINDOW_KEY)
            : _serviceProvider.GetRequiredKeyedService<IDialogWindow>(windowName);

    private (IView View, IDialogAware Aware) ResolveDialogViewModel(string name)
    {
        var view = _serviceProvider.GetRequiredKeyedService<IView>(name);
        if (view.DataContext is IDialogAware aware)
        {
            
        }
        else
        {
            aware = _serviceProvider.GetRequiredKeyedService<IDialogAware>(name);
            view.DataContext = aware;
        }
        return (view, aware);
    }

    private (IDialogWindow Window, IDialogAware ViewModel) PrepareDialog(string name, string? windowName = null)
    {
        var window = ResolveDialogWindow(windowName);
        var (view, viewModel) = ResolveDialogViewModel(name);

        window.Title = viewModel.Title;
        window.Content = view;
        window.DataContext = viewModel;

        return (window, viewModel);
    }
    protected Task<IDialogResult> HandleCloseInternalAsync(
        IDialogWindowBase dialogWindow,
        IDialogAware dialogAware,
        Func<IDialogResult, object>? mainWindowBuilder = null)
    {
        var tcs = new TaskCompletionSource<IDialogResult>();
        IDialogResult? pendingResult = null;
        bool isClosingHandled = false;

        async Task RequestCloseHandler(object? sender, DialogCloseEventArgs args)
        {
            try
            {
                await dialogAware.OnDialogClosingAsync(args.DialogResult, args.CancellationToken);
                args.CancellationToken.ThrowIfCancellationRequested();

                pendingResult = args.DialogResult;
                isClosingHandled = true;
                dialogWindow.Close();
            }
            catch (OperationCanceledException)
            {
                pendingResult = DialogResult.Cancelled;
                isClosingHandled = true;
            }
        }

        async void WindowClosingHandler(object? sender, WindowClosingEventArgs e)
        {
            if (!isClosingHandled)
            {
                e.Cancel = true;

                try
                {
                    var result = pendingResult ?? new DialogResult(DialogButtonResult.None);
                    // Closing the window by means other than RequestCloseAsync does not support cancellation.
                    await dialogAware.OnDialogClosingAsync(result, CancellationToken.None);
                    pendingResult = result;
                    isClosingHandled = true;

                    if (SynchronizationContext.Current is not null)
                    {
                        SynchronizationContext.Current.Post(_ =>
                        {
                            dialogWindow.Close();
                        }, null);
                    }
                    else
                    {
                        throw new NotSupportedException(
                            "Cannot close the window on the current thread: no SynchronizationContext detected. " +
                            "This operation must be performed on the UI thread. " +
                            "Please switch to the UI thread using Dispatcher or an appropriate synchronization context before calling this method.");
                    }
                }
                catch (OperationCanceledException)
                {
                    pendingResult = DialogResult.Cancelled;
                }
                catch (NotSupportedException)
                {
                    throw;
                }
                catch (Exception)
                {
                    pendingResult ??= new DialogResult(DialogButtonResult.None);
                    isClosingHandled = true;
                    dialogWindow.Close();
                }

                return;
            }

            if (mainWindowBuilder != null)
            {
                try
                {
                    var result = pendingResult ?? new DialogResult(DialogButtonResult.None);
                    var mainWindow = mainWindowBuilder.Invoke(result);
                    _platformService.ShowMainWindow(mainWindow);
                }
                catch (Exception)
                {
                    pendingResult ??= new DialogResult(DialogButtonResult.None);
                }
            }
        }

        void ClosedHandler(object? sender, EventArgs e)
        {
            dialogWindow.Closed -= ClosedHandler;
            dialogAware.RequestCloseAsync -= RequestCloseHandler;

            var finalResult = pendingResult ?? new DialogResult(DialogButtonResult.None);
            tcs.TrySetResult(finalResult);
        }

        dialogAware.RequestCloseAsync += RequestCloseHandler;
        dialogWindow.Closed += ClosedHandler;
        _platformService.AttachClosing(dialogWindow, WindowClosingHandler);

        return tcs.Task.ContinueWith(async asyncTask =>
        {
            var result = asyncTask.Result;

            try
            {
                await dialogAware.OnDialogClosedAsync(result, CancellationToken.None);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"OnDialogClosedAsync failed: {ex}");
            }

            return result;
        }).Unwrap();
    }



    //protected Task<IDialogResult> HandleClosingInternalAsync<TMainWindow>(
    //    IDialogWindowBase baseWindow,
    //    IDialogAware dialogAware,
    //    Func<IDialogResult, TMainWindow>? mainWindowBuilder) where TMainWindow : class
    //{
    //    var tcs = new TaskCompletionSource<IDialogResult>();
    //    IDialogResult? pendingResult = null;

    //    void HandleClosing(object? sender, WindowClosingEventArgs e)
    //    {
    //        try
    //        {
    //            if (mainWindowBuilder != null)
    //            {
    //                var result = pendingResult ?? new DialogResult(DialogButtonResult.None);
    //                var mainWindow = mainWindowBuilder.Invoke(result);
    //                _platformService.SetMainWindow(mainWindow);
    //            }
    //        }
    //        catch (OperationCanceledException)
    //        {
    //            pendingResult = DialogResult.Cancelled;
    //            e.Cancel = true;
    //        }
    //        catch (Exception)
    //        {
    //            pendingResult = new DialogResult(DialogButtonResult.None);
    //        }
    //    }

    //    async Task RequestCloseHandler(object? sender, DialogCloseEventArgs args)
    //    {
    //        try
    //        {
    //            await dialogAware.OnDialogClosingAsync(args.DialogResult, args.CancellationToken);
    //            args.CancellationToken.ThrowIfCancellationRequested();

    //            pendingResult = args.DialogResult;
    //            baseWindow.Close();
    //        }
    //        catch (OperationCanceledException)
    //        {
    //            pendingResult = DialogResult.Cancelled;
    //        }
    //    }

    //    void ClosedHandler(object? sender, EventArgs e)
    //    {
    //        baseWindow.Closed -= ClosedHandler;
    //        dialogAware.RequestCloseAsync -= RequestCloseHandler;
    //        tcs.TrySetResult(pendingResult ?? new DialogResult(DialogButtonResult.None));
    //    }
    //    dialogAware.RequestCloseAsync += RequestCloseHandler;
    //    baseWindow.Closed += ClosedHandler;
    //    _platformService.AttachClosing(baseWindow, HandleClosing);

    //    return tcs.Task;
    //}
}

