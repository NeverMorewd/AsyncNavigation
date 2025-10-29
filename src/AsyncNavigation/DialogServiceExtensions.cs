using AsyncNavigation.Abstractions;

namespace AsyncNavigation;

public static class DialogServiceExtensions
{
    public static void ShowView(this IDialogService dialogService,
        string viewName,
        string? containerName = null,
        IDialogParameters? parameters = null,
        Action<IDialogResult>? callBack = null,
        CancellationToken cancellationToken = default)
    {
        dialogService.Show(viewName, containerName, parameters, callBack, cancellationToken);
    }

    public static void ShowWindow(this IDialogService dialogService,
        string windowName,
        IDialogParameters? parameters = null,
        Action<IDialogResult>? callBack = null,
        CancellationToken cancellationToken = default)
    {
        dialogService.Show(windowName, parameters, callBack, cancellationToken);
    }
    public static IDialogResult ShowViewDialog(this IDialogService dialogService,
        string viewName,
        string? containerName = null,
        IDialogParameters? parameters = null,
        CancellationToken cancellationToken = default)
    {
        return dialogService.ShowDialog(viewName, containerName, parameters, cancellationToken);
    }

    public static IDialogResult ShowWindowDialog(this IDialogService dialogService,
        string windowName,
        IDialogParameters? parameters = null,
        CancellationToken cancellationToken = default)
    {
        return dialogService.ShowDialog(windowName, parameters, cancellationToken);
    }

    public static Task<IDialogResult> ShowViewDialogAsync(this IDialogService dialogService,
        string viewName,
        string? containerName = null,
        IDialogParameters? parameters = null,
        CancellationToken cancellationToken = default)
    {
        return dialogService.ShowDialogAsync(viewName, containerName, parameters, cancellationToken);
    }

    public static Task<IDialogResult> ShowWindowDialogAsync(this IDialogService dialogService,
        string windowName,
        IDialogParameters? parameters = null,
        CancellationToken cancellationToken = default)
    {
        return dialogService.ShowDialogAsync(windowName, parameters, cancellationToken);
    }

    public static Task FrontShowViewAsync<TMainWindow>(this IDialogService dialogService,
        string viewName,
        Func<IDialogResult, TMainWindow?> mainWindowBuilder,
        string? containerName = null,
        IDialogParameters? parameters = null,
        CancellationToken cancellationToken = default) where TMainWindow : class
    {
        return dialogService.FrontShowAsync(viewName, mainWindowBuilder, containerName, parameters, cancellationToken);
    }

    public static Task FrontShowWindowAsync<TMainWindow>(this IDialogService dialogService,
        string windowName,
        Func<IDialogResult, TMainWindow?> mainWindowBuilder,
        IDialogParameters? parameters = null,
        CancellationToken cancellationToken = default) where TMainWindow : class
    {
        return dialogService.FrontShowAsync(windowName, mainWindowBuilder, parameters, cancellationToken);
    }
}
