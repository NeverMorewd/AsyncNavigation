namespace AsyncNavigation.Abstractions;

/// <summary>
/// Allows a view model to block or confirm a navigation request before it proceeds.
/// Implement this interface alongside <see cref="INavigationAware"/> to intercept
/// navigations away from the current view (e.g., unsaved-changes confirmation).
/// </summary>
/// <example>
/// <code>
/// public class EditViewModel : NavigationAwareBase, INavigationGuard
/// {
///     public async Task&lt;bool&gt; CanNavigateAsync(NavigationContext context, CancellationToken ct)
///     {
///         if (!HasUnsavedChanges) return true;
///         return await _dialogService.ConfirmAsync("Discard unsaved changes?", ct);
///     }
/// }
/// </code>
/// </example>
public interface INavigationGuard
{
    /// <summary>
    /// Called before navigating away from the current view.
    /// Return <see langword="true"/> to allow the navigation to proceed,
    /// or <see langword="false"/> to cancel it.
    /// </summary>
    /// <param name="context">The navigation context for the incoming navigation request.</param>
    /// <param name="cancellationToken">Token to cancel the guard check itself.</param>
    Task<bool> CanNavigateAsync(NavigationContext context, CancellationToken cancellationToken);
}
