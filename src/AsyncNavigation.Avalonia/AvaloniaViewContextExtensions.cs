using AsyncNavigation.Abstractions;
using Avalonia.Controls;
using Avalonia.Input.Platform;
using Avalonia.Platform.Storage;

namespace AsyncNavigation.Avalonia;

/// <summary>
/// Extension methods that expose Avalonia-specific capabilities from an <see cref="IViewContext"/>.
/// </summary>
/// <remarks>
/// A ViewModel that receives an <see cref="IViewContext"/> via <see cref="IViewAware.OnViewAttached"/>
/// can access these members after adding a <c>using AsyncNavigation.Avalonia;</c> directive:
/// <code>
/// private IViewContext? _viewContext;
///
/// public void OnViewAttached(IViewContext context) => _viewContext = context;
///
/// [ReactiveCommand]
/// private async Task PickFile()
/// {
///     if (_viewContext is null) return;
///     var files = await _viewContext.GetStorageProvider()
///         .OpenFilePickerAsync(new FilePickerOpenOptions { AllowMultiple = false });
/// }
/// </code>
/// </remarks>
public static class AvaloniaViewContextExtensions
{
    /// <summary>Gets the <see cref="TopLevel"/> associated with the attached view.</summary>
    public static TopLevel GetTopLevel(this IViewContext context)
        => ((ViewContext)context).TopLevel;

    /// <summary>Gets the <see cref="IStorageProvider"/> for file system access dialogs.</summary>
    public static IStorageProvider GetStorageProvider(this IViewContext context)
        => ((ViewContext)context).StorageProvider;

    /// <summary>Gets the <see cref="IClipboard"/> for clipboard operations, or <c>null</c> if unavailable.</summary>
    public static IClipboard? GetClipboard(this IViewContext context)
        => ((ViewContext)context).Clipboard;

    /// <summary>Gets the <see cref="ILauncher"/> for opening URIs and files, or <c>null</c> if unavailable.</summary>
    public static ILauncher? GetLauncher(this IViewContext context)
        => ((ViewContext)context).Launcher;
}
