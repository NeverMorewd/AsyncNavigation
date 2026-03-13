namespace AsyncNavigation.Abstractions;

/// <summary>
/// Implemented by view models that need access to platform-specific view capabilities
/// (e.g., file dialogs, clipboard, the host window).
/// The framework calls <see cref="OnViewAttached"/> after navigation completes and
/// <see cref="OnViewDetached"/> when the view is removed or the region is disposed.
/// </summary>
public interface IViewAware
{
    void OnViewAttached(IViewContext context);
    void OnViewDetached();
}
