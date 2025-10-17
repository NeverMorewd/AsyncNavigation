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
    protected Task<IDialogResult> HandleCloseInternalAsyncOld(
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


    protected Task<IDialogResult> HandleCloseInternalAsync(
        IDialogWindowBase dialogWindow,
        IDialogAware dialogAware,
        Func<IDialogResult, object>? mainWindowBuilder = null)
    {
        var tcs = new TaskCompletionSource<IDialogResult>();
        var closeState = new DialogCloseState();

        dialogAware.RequestCloseAsync += OnRequestClose;
        dialogWindow.Closed += OnClosed;
        _platformService.AttachClosing(dialogWindow, OnWindowClosing);

        return tcs.Task.ContinueWith(FinalizeDialogClose, TaskScheduler.Default).Unwrap();

        async Task OnRequestClose(object? sender, DialogCloseEventArgs args)
        {
            try
            {
                await dialogAware.OnDialogClosingAsync(args.DialogResult, args.CancellationToken);
                args.CancellationToken.ThrowIfCancellationRequested();

                closeState.SetResult(args.DialogResult);
                dialogWindow.Close();
            }
            catch (OperationCanceledException)
            {
                closeState.SetCancelled();
            }
        }

        async void OnWindowClosing(object? sender, WindowClosingEventArgs e)
        {
            if (closeState.IsHandled)
                return;

            e.Cancel = true;

            try
            {
                var result = closeState.GetResultOrDefault();
                await dialogAware.OnDialogClosingAsync(result, CancellationToken.None);

                closeState.SetResult(result);
                CloseOnUIThread(dialogWindow);
            }
            catch (OperationCanceledException)
            {
                closeState.SetCancelled();
            }
            catch (NotSupportedException)
            {
                throw;
            }
            catch (Exception)
            {
                closeState.SetResultIfNotSet(new DialogResult(DialogButtonResult.None));
                dialogWindow.Close();
            }
        }

        void OnClosed(object? sender, EventArgs e)
        {
            dialogWindow.Closed -= OnClosed;
            dialogAware.RequestCloseAsync -= OnRequestClose;

            ShowMainWindowIfNeeded(closeState.GetResultOrDefault());
            tcs.TrySetResult(closeState.GetResultOrDefault());
        }

        async Task<IDialogResult> FinalizeDialogClose(Task<IDialogResult> resultTask)
        {
            var result = resultTask.Result;

            try
            {
                await dialogAware.OnDialogClosedAsync(result, CancellationToken.None);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"OnDialogClosedAsync failed: {ex}");
            }

            return result;
        }

        void ShowMainWindowIfNeeded(IDialogResult result)
        {
            if (mainWindowBuilder == null)
                return;

            try
            {
                var mainWindow = mainWindowBuilder.Invoke(result);
                _platformService.ShowMainWindow(mainWindow);
            }
            catch (Exception)
            {
                
            }
        }
    }
    private static void CloseOnUIThread(IDialogWindowBase dialogWindow)
    {
        if (SynchronizationContext.Current is not null)
        {
            SynchronizationContext.Current.Post(_ => dialogWindow.Close(), null);
        }
        else
        {
            throw new NotSupportedException(
                "Cannot close the window on the current thread: no SynchronizationContext detected. " +
                "This operation must be performed on the UI thread. " +
                "Please switch to the UI thread using Dispatcher or an appropriate synchronization context before calling this method.");
        }
    }

    private class DialogCloseState
    {
        private IDialogResult? _result;
        private bool _isHandled;

        public bool IsHandled => _isHandled;

        public void SetResult(IDialogResult result)
        {
            _result = result;
            _isHandled = true;
        }

        public void SetCancelled()
        {
            _result = DialogResult.Cancelled;
            _isHandled = true;
        }

        public void SetResultIfNotSet(IDialogResult result)
        {
            _result ??= result;
            _isHandled = true;
        }

        public IDialogResult GetResultOrDefault()
        {
            return _result ?? new DialogResult(DialogButtonResult.None);
        }
    }
}

