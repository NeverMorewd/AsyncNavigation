using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Input;
using Avalonia.Input.Platform;
using Avalonia.Platform.Storage;

namespace AsyncNavigation.Avalonia;

/// <summary>
/// Default implementation of <see cref="ITopLevelProvider"/> that resolves
/// the active <see cref="TopLevel"/> from the current Avalonia application lifetime.
/// </summary>
/// <remarks>
/// Resolution strategy (when no custom resolver is supplied):
/// <list type="bullet">
///   <item>
///     <b>Classic desktop</b> (<see cref="IClassicDesktopStyleApplicationLifetime"/>):
///     Returns the first active window, falling back to <c>MainWindow</c>,
///     then to the most recently shown window.
///   </item>
///   <item>
///     <b>Single-view</b> (<see cref="ISingleViewApplicationLifetime"/>):
///     Walks up the visual tree of <c>MainView</c> to find its owning TopLevel.
///   </item>
///   <item>
///     Any other lifetime: returns <see langword="null"/>.
///   </item>
/// </list>
/// A custom <paramref name="topLevelResolver"/> can be supplied to override the default
/// lookup — primarily useful in tests (e.g. headless Avalonia, where no application
/// lifetime is registered) and in single-page applications that manage their own root.
/// </remarks>
internal sealed class TopLevelProvider : ITopLevelProvider
{
    private readonly Func<TopLevel?> _resolver;

    internal TopLevelProvider(Func<TopLevel?>? topLevelResolver = null)
    {
        _resolver = topLevelResolver ?? ResolveFromAppLifetime;
    }

    // Default production resolver — walks ApplicationLifetime to find the active window.
    private static TopLevel? ResolveFromAppLifetime() =>
        Application.Current?.ApplicationLifetime switch
        {
            IClassicDesktopStyleApplicationLifetime desktop =>
                desktop.Windows.FirstOrDefault(w => w.IsActive)
                ?? desktop.MainWindow
                ?? desktop.Windows.LastOrDefault(),

            ISingleViewApplicationLifetime { MainView: { } view } =>
                TopLevel.GetTopLevel(view),

            _ => null
        };

    /// <inheritdoc/>
    public TopLevel? GetTopLevel() => _resolver();

    /// <inheritdoc/>
    public IClipboard? Clipboard => GetTopLevel()?.Clipboard;

    /// <inheritdoc/>
    public IStorageProvider StorageProvider =>
        GetTopLevel()?.StorageProvider
        ?? throw new InvalidOperationException(
            "No active TopLevel is available. Ensure at least one window is shown before accessing StorageProvider.");

    /// <inheritdoc/>
    public IFocusManager? FocusManager => GetTopLevel()?.FocusManager;
}
