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

    public void Show(string name,
        string? windowName,
        IDialogParameters? parameters,
        Action<IDialogResult>? callback,
        CancellationToken cancellationToken)
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
    
    public async Task<IDialogResult> ShowDialogAsync(string name,
        string? windowName,
        IDialogParameters? parameters,
        CancellationToken cancellationToken)
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
        string? windowName, 
        IDialogParameters? parameters, 
        CancellationToken cancellationToken)
    {
        var showTask = ShowDialogAsync(name, windowName, parameters, cancellationToken);
        return _platformService.WaitOnDispatcher(showTask);
    }

    public async Task FrontShowAsync<TWindow>(string name,
        Func<IDialogResult, TWindow?> mainWindowBuilder,
        string? windowName,
        IDialogParameters? parameters,
        CancellationToken cancellationToken) where TWindow:class
    {
        var (dialogWindow, aware) = PrepareDialog(name, windowName);

        var openTask = aware.OnDialogOpenedAsync(parameters, cancellationToken);
        await openTask;

        var closeTask = HandleCloseInternalAsync(dialogWindow, aware, mainWindowBuilder);

        _platformService.ShowMainWindow(dialogWindow);
        _platformService.Show(dialogWindow, false);
        await closeTask;
    }

    public void Show(string windowName, 
        IDialogParameters? parameters, 
        Action<IDialogResult>? callback, 
        CancellationToken cancellationToken)
    {
        var (dialogWindow, aware) = PrepareDialogWindow(windowName);

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

    public async Task<IDialogResult> ShowDialogAsync(string windowName, 
        IDialogParameters? parameters, 
        CancellationToken cancellationToken)
    {
        var (dialogWindow, aware) = PrepareDialogWindow(windowName);

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

    public IDialogResult ShowDialog(string windowName, 
        IDialogParameters? parameters, 
        CancellationToken cancellationToken)
    {
        var showTask = ShowDialogAsync(windowName, parameters, cancellationToken);
        return _platformService.WaitOnDispatcher(showTask);
    }

    public async Task FrontShowAsync<TWindow>(string windowName, 
        Func<IDialogResult, TWindow?> mainWindowBuilder, 
        IDialogParameters? parameters, 
        CancellationToken cancellationToken) where TWindow : class
    {
        var (dialogWindow, aware) = PrepareDialogWindow(windowName);

        var openTask = aware.OnDialogOpenedAsync(parameters, cancellationToken);
        await openTask;

        _ = HandleCloseInternalAsync(dialogWindow, aware, mainWindowBuilder);
        _platformService.ShowMainWindow(dialogWindow);
        _platformService.Show(dialogWindow, false);
    }


    private IDialogWindow ResolveDialogWindow(string? windowName) =>
        string.IsNullOrEmpty(windowName)
            ? _serviceProvider.GetRequiredKeyedService<IDialogWindow>(NavigationConstants.DEFAULT_DIALOG_WINDOW_KEY)
            : _serviceProvider.GetRequiredKeyedService<IDialogWindow>(windowName);
    private (IView View, IDialogAware Aware) ResolveDialogView(string name)
    {
        var view = _serviceProvider.GetRequiredKeyedService<IView>(name);
        if (view.DataContext is IDialogAware aware)
        {
            return (view, aware);
        }
        else
        {
            aware = ResolveDialogViewModel(name);
            view.DataContext = aware;
            return (view, aware);
        }
    }
    private IDialogAware ResolveDialogViewModel(string name)
    {
        return _serviceProvider.GetRequiredKeyedService<IDialogAware>(name);
    }
    private (IDialogWindow Window, IDialogAware ViewModel) PrepareDialog(string name, string? windowName)
    {
        var window = ResolveDialogWindow(windowName);
        var (view, viewModel) = ResolveDialogView(name);

        window.Title = viewModel.Title;
        window.Content = view;
        window.DataContext = viewModel;

        return (window, viewModel);
    }
    private (IDialogWindow Window, IDialogAware ViewModel) PrepareDialogWindow(string windowName)
    {
        var window = ResolveDialogWindow(windowName);
        if (window.DataContext is IDialogAware aware)
        {
            window.Title = aware.Title;
            return (window, aware);
        }
        else
        {
            aware = ResolveDialogViewModel(windowName);
            window.DataContext = aware;
            window.Title = aware.Title;
            return (window, aware);
        }
    }

    private async Task<IDialogResult> HandleCloseInternalAsync(
        IDialogWindowBase dialogWindow,
        IDialogAware dialogAware,
        Func<IDialogResult, object?>? mainWindowBuilder = null)
    {
        var tcs = new TaskCompletionSource<IDialogResult>();
        var closeState = new DialogCloseState();

        dialogAware.RequestCloseAsync += OnRequestClose;
        dialogWindow.Closed += OnClosed;
        _platformService.AttachClosing(dialogWindow, OnWindowClosing);

        var result = await tcs.Task;
        await FinalizeDialogCloseAsync(result);
        return result;

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
                var state = closeState.GetResultOrDefault();
                await dialogAware.OnDialogClosingAsync(state, CancellationToken.None);

                closeState.SetResult(state);
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
        async Task<IDialogResult> FinalizeDialogCloseAsync(IDialogResult dialogResult)
        {
            try
            {
                await dialogAware.OnDialogClosedAsync(dialogResult, CancellationToken.None);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"OnDialogClosedAsync failed: {ex}");
            }
            return result;
        }
        void ShowMainWindowIfNeeded(IDialogResult dialogResult)
        {
            if (mainWindowBuilder == null)
                return;

            var mainWindow = mainWindowBuilder.Invoke(dialogResult);
            if (mainWindow is not null)
            {
                _platformService.ShowMainWindow(mainWindow);
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

        public bool IsHandled { get; private set; }

        public void SetResult(IDialogResult result)
        {
            _result = result;
            IsHandled = true;
        }

        public void SetCancelled()
        {
            _result = DialogResult.Cancelled;
            IsHandled = true;
        }

        public void SetResultIfNotSet(IDialogResult result)
        {
            _result ??= result;
            IsHandled = true;
        }

        public IDialogResult GetResultOrDefault()
        {
            return _result ?? new DialogResult(DialogButtonResult.None);
        }
    }
}

