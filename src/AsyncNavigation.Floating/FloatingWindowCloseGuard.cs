namespace AsyncNavigation.Floating;

/// <summary>
/// Encapsulates the "cancel the OS window's close, then raise <c>CloseRequested</c> instead"
/// dance shared by every <see cref="IFloatingWindowHost"/> backed by a real window: closing the
/// window via the OS chrome (or Alt+F4 etc.) must not close it outright - it has to be turned
/// into a request that the caller can still veto or handle asynchronously via
/// <c>CloseAsync</c>/<c>DisposeAsync</c>. Shared so this exact state transition - easy to get
/// subtly wrong per platform - only has to be implemented once.
/// </summary>
public sealed class FloatingWindowCloseGuard
{
    private volatile bool _allowClose;

    /// <summary>Call before actually closing the window from <c>CloseAsync</c>/<c>DisposeAsync</c>.</summary>
    public void AllowClose() => _allowClose = true;

    /// <summary>
    /// Call from the window's own Closing handler. Returns true when the close was not
    /// explicitly requested via <see cref="AllowClose"/> - the caller must then cancel the
    /// window's own close and raise its <c>CloseRequested</c> event instead.
    /// </summary>
    public bool ShouldCancelAndNotify() => !_allowClose;
}
