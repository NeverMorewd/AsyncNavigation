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
        var (dialogWindow, aware) = PrePareDialog(name, windowName);

        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            await aware.OnDialogOpenedAsync(parameters, cancellationToken);
            cancellationToken.ThrowIfCancellationRequested();
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return DialogResult.Cancelled;
        }
        var closeTask = _platformService.HandleDialogCloseAsync(dialogWindow, aware);
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
        CancellationToken cancellationToken = default,
        Action<IDialogResult>? callback = null)
    {
        var (dialogWindow, aware) = PrePareDialog(name, windowName);

        aware.OnDialogOpenedAsync(parameters, cancellationToken);
        var closeTask = _platformService.HandleDialogCloseAsync(dialogWindow, aware);
        closeTask.ContinueWith(t =>
        {
            if (t.IsCompleted)
            {
                callback?.Invoke(t.Result);
            }
        });
        _platformService.Show(dialogWindow, false);
    }
    private IWindowBase ResolveDialogWindow(string? windowName) =>
        string.IsNullOrEmpty(windowName)
            ? _serviceProvider.GetRequiredKeyedService<IWindowBase>(NavigationConstants.DEFAULT_DIALOG_WINDOW_KEY)
            : _serviceProvider.GetRequiredKeyedService<IWindowBase>(windowName);

    private (IView View, IDialogAware Aware) ResolveDialogViewModel(string name)
    {
        var aware = _serviceProvider.GetRequiredKeyedService<IDialogAware>(name);
        var view = _serviceProvider.GetRequiredKeyedService<IView>(name);
        view.DataContext = aware;
        return (view, aware);
    }

    private (IWindowBase DialogWindow, IDialogAware Aware) PrePareDialog(string name, string? windowName = null)
    {
        var dialogWindow = ResolveDialogWindow(windowName);
        var (view, aware) = ResolveDialogViewModel(name);

        dialogWindow.Title = aware.Title;
        dialogWindow.Content = view;
        dialogWindow.DataContext = aware;
        return (dialogWindow, aware);
    }
}

