using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Input.Platform;
using Avalonia.Platform.Storage;

namespace AsyncNavigation.Avalonia;

/// <summary>
/// Provides access to the current Avalonia <see cref="TopLevel"/> and its platform services,
/// allowing ViewModels to consume clipboard, file-picker, and focus features without
/// taking a direct dependency on Avalonia view types.
/// </summary>
/// <remarks>
/// Register this service once via <c>services.AddNavigationSupport()</c>.
/// Inject <see cref="ITopLevelProvider"/> into ViewModels instead of passing
/// <see cref="TopLevel"/> / <see cref="Window"/> instances directly.
///
/// <code>
/// // ViewModel usage
/// public class MyViewModel
/// {
///     private readonly ITopLevelProvider _topLevel;
///
///     public MyViewModel(ITopLevelProvider topLevel)
///         => _topLevel = topLevel;
///
///     public async Task PickFileAsync()
///     {
///         var files = await _topLevel.StorageProvider
///             .OpenFilePickerAsync(new FilePickerOpenOptions());
///         // ...
///     }
///
///     public async Task CopyTextAsync(string text)
///         => await (_topLevel.Clipboard?.SetTextAsync(text) ?? Task.CompletedTask);
/// }
/// </code>
/// </remarks>
public interface ITopLevelProvider
{
    /// <summary>
    /// Returns the current active <see cref="TopLevel"/> (typically the focused
    /// <see cref="Window"/>), or <see langword="null"/> when no window is available.
    /// </summary>
    TopLevel? GetTopLevel();

    /// <summary>
    /// Gets the <see cref="IClipboard"/> from the current <see cref="TopLevel"/>,
    /// or <see langword="null"/> if no active TopLevel exists or the platform does
    /// not support clipboard access.
    /// </summary>
    IClipboard? Clipboard { get; }

    /// <summary>
    /// Gets the <see cref="IStorageProvider"/> (file/folder picker) from the
    /// current <see cref="TopLevel"/>.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// Thrown when no active <see cref="TopLevel"/> is available.
    /// </exception>
    IStorageProvider StorageProvider { get; }

    /// <summary>
    /// Gets the <see cref="IFocusManager"/> from the current <see cref="TopLevel"/>,
    /// or <see langword="null"/> if no active TopLevel exists.
    /// </summary>
    IFocusManager? FocusManager { get; }
}
