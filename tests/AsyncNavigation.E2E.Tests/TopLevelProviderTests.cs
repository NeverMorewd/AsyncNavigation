using AsyncNavigation.Avalonia;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Microsoft.Extensions.DependencyInjection;

namespace AsyncNavigation.E2E.Tests;

/// <summary>
/// Tests for <see cref="ITopLevelProvider"/>.
///
/// In Avalonia Headless XUnit the app runs without an ApplicationLifetime, so
/// <c>Application.Current.ApplicationLifetime</c> is null and the default production
/// resolver cannot find windows.  Every test that exercises runtime behaviour therefore
/// supplies its own <c>topLevelResolver</c> lambda — the same escape hatch that
/// production apps may use for exotic hosting environments.
/// </summary>
public class TopLevelProviderTests
{
    // -----------------------------------------------------------------------
    // Helpers
    // -----------------------------------------------------------------------

    private Window? _window;

    /// <summary>
    /// Builds an <see cref="ITopLevelProvider"/> whose TopLevel source is
    /// <c>_window</c> (controlled per test).
    /// </summary>
    private ITopLevelProvider CreateProvider() =>
        new ServiceCollection()
            .AddNavigationSupport(topLevelResolver: () => _window)
            .BuildServiceProvider()
            .GetRequiredService<ITopLevelProvider>();

    // -----------------------------------------------------------------------
    // DI registration — no Avalonia runtime required
    // -----------------------------------------------------------------------

    [Fact]
    public void AddNavigationSupport_RegistersITopLevelProvider()
    {
        var services = new ServiceCollection();
        services.AddNavigationSupport();
        var sp = services.BuildServiceProvider();

        Assert.NotNull(sp.GetService<ITopLevelProvider>());
    }

    [Fact]
    public void AddNavigationSupport_ITopLevelProvider_IsSingleton()
    {
        var sp = new ServiceCollection()
            .AddNavigationSupport()
            .BuildServiceProvider();

        Assert.Same(
            sp.GetRequiredService<ITopLevelProvider>(),
            sp.GetRequiredService<ITopLevelProvider>());
    }

    // -----------------------------------------------------------------------
    // GetTopLevel
    // -----------------------------------------------------------------------

    [AvaloniaFact]
    public void GetTopLevel_WhenWindowIsSet_ReturnsWindow()
    {
        _window = new Window();
        _window.Show();

        Assert.Equal(_window, CreateProvider().GetTopLevel());
    }

    [AvaloniaFact]
    public void GetTopLevel_WhenWindowIsNull_ReturnsNull()
    {
        _window = null;

        Assert.Null(CreateProvider().GetTopLevel());
    }

    // -----------------------------------------------------------------------
    // Clipboard
    // -----------------------------------------------------------------------

    [AvaloniaFact]
    public void Clipboard_WhenWindowAvailable_ReturnsWindowClipboard()
    {
        _window = new Window();
        _window.Show();

        Assert.Equal(_window.Clipboard, CreateProvider().Clipboard);
    }

    [AvaloniaFact]
    public void Clipboard_WhenNoWindowAvailable_ReturnsNull()
    {
        _window = null;

        Assert.Null(CreateProvider().Clipboard);
    }

    // -----------------------------------------------------------------------
    // StorageProvider
    // -----------------------------------------------------------------------

    [AvaloniaFact]
    public void StorageProvider_WhenWindowAvailable_ReturnsWindowStorageProvider()
    {
        _window = new Window();
        _window.Show();

        Assert.Equal(_window.StorageProvider, CreateProvider().StorageProvider);
    }

    [AvaloniaFact]
    public void StorageProvider_WhenNoWindowAvailable_ThrowsInvalidOperationException()
    {
        _window = null;

        Assert.Throws<InvalidOperationException>(() => _ = CreateProvider().StorageProvider);
    }

    // -----------------------------------------------------------------------
    // FocusManager
    // -----------------------------------------------------------------------

    [AvaloniaFact]
    public void FocusManager_WhenWindowAvailable_ReturnsWindowFocusManager()
    {
        _window = new Window();
        _window.Show();

        Assert.Equal(_window.FocusManager, CreateProvider().FocusManager);
    }

    [AvaloniaFact]
    public void FocusManager_WhenNoWindowAvailable_ReturnsNull()
    {
        _window = null;

        Assert.Null(CreateProvider().FocusManager);
    }
}
