using AsyncNavigation;
using AsyncNavigation.Abstractions;
using AsyncNavigation.Core;

namespace AsyncNavigation.Avalonia;

/// <summary>
/// Avalonia-specific dialog service. Extends <see cref="DialogService"/> with
/// <see cref="IAvaloniaDialogService"/> which exposes the front-show pattern under
/// the clearer <c>FrontShowViewAsync</c> / <c>FrontShowWindowAsync</c> names.
/// </summary>
internal sealed class AvaloniaDialogService : DialogService, IAvaloniaDialogService
{
    public AvaloniaDialogService(IServiceProvider serviceProvider, IPlatformService platformService)
        : base(serviceProvider, platformService)
    {
    }

    Task IAvaloniaDialogService.FrontShowViewAsync<TWindow>(
        string viewName,
        Func<IDialogResult, TWindow?> mainWindowBuilder,
        string? containerName,
        IDialogParameters? parameters,
        CancellationToken cancellationToken) where TWindow : class
        => FrontShowAsync(viewName, mainWindowBuilder, containerName, parameters, cancellationToken);

    Task IAvaloniaDialogService.FrontShowWindowAsync<TWindow>(
        string windowName,
        Func<IDialogResult, TWindow?> mainWindowBuilder,
        IDialogParameters? parameters,
        CancellationToken cancellationToken) where TWindow : class
        => FrontShowAsync(windowName, mainWindowBuilder, parameters, cancellationToken);
}
