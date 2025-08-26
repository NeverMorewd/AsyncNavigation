using AsyncNavigation.Abstractions;
using AsyncNavigation.Core;
using Microsoft.Extensions.DependencyInjection;
using System.Xml.Linq;

namespace AsyncNavigation;

public class DialogService : IDialogService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IDialogPlatformService _platformService;

    public DialogService(IServiceProvider serviceProvider, IDialogPlatformService platformService)
    {
        _serviceProvider = serviceProvider;
        _platformService = platformService;
    }

    public async Task<IDialogResult> ShowDialogAsync(string name,
        string? windowName = null,
        IDialogParameters? parameters = null,
        object? owner = null, 
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
        var closeTask = _platformService.HandleCloseAsync(dialogWindow, aware);
        await _platformService.ShowAsync(dialogWindow, true, owner);
        return await closeTask;
    }
    public IDialogResult ShowDialog(string name, 
        string? windowName = null, 
        IDialogParameters? parameters = null, 
        object? owner = null, 
        CancellationToken cancellationToken = default)
    {
        var showTask = ShowDialogAsync(name, windowName, parameters, owner, cancellationToken);
        return _platformService.WaitOnUIThread(showTask);
    }
    public void Show(string name, 
        string? windowName = null, 
        IDialogParameters? parameters = null, 
        object? owner = null, 
        CancellationToken cancellationToken = default,
        Action<IDialogResult>? callback = null)
    {
        var (dialogWindow, aware) = PrePareDialog(name, windowName);

        aware.OnDialogOpenedAsync(parameters, cancellationToken);
        var closeTask = _platformService.HandleCloseAsync(dialogWindow, aware);
        closeTask.ContinueWith(t =>
        {
            callback?.Invoke(_platformService.WaitOnUIThread(t));
        });
        var showTask = _platformService.ShowAsync(dialogWindow, false, owner);
        _platformService.WaitOnUIThread(showTask);
    }
    private IDialogWindowBase ResolveDialogWindow(string? windowName) =>
        string.IsNullOrEmpty(windowName)
            ? _serviceProvider.GetRequiredKeyedService<IDialogWindowBase>(NavigationConstants.DEFAULT_DIALOG_WINDOW_KEY)
            : _serviceProvider.GetRequiredKeyedService<IDialogWindowBase>(windowName);

    private (IView View, IDialogAware Aware) ResolveDialogViewModel(string name)
    {
        var aware = _serviceProvider.GetRequiredKeyedService<IDialogAware>(name);
        var view = _serviceProvider.GetRequiredKeyedService<IView>(name);
        view.DataContext = aware;
        return (view, aware);
    }

    private (IDialogWindowBase DialogWindow, IDialogAware Aware) PrePareDialog(string name, string? windowName = null)
    {
        var dialogWindow = ResolveDialogWindow(windowName);
        var (view, aware) = ResolveDialogViewModel(name);

        dialogWindow.Title = aware.Title;
        dialogWindow.Content = view;
        dialogWindow.DataContext = aware;
        return (dialogWindow, aware);
    }

}

