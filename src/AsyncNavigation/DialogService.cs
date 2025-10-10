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

    protected Task<IDialogResult> HandleCloseInternalAsync(IWindowBase baseWindow, IDialogAware dialogAware)
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
        var aware = _serviceProvider.GetRequiredKeyedService<IDialogAware>(name);
        var view = _serviceProvider.GetRequiredKeyedService<IView>(name);
        view.DataContext = aware;
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

}

