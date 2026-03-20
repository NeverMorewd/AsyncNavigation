namespace AsyncNavigation.Abstractions;

/// <summary>
/// Globally intercepts navigation requests across all regions.
/// Register an implementation via <c>RegisterNavigationInterceptor&lt;T&gt;()</c>.
/// </summary>
/// <remarks>
/// Interceptors run in registration order and are invoked for every navigation request
/// regardless of region or view. Common uses: authentication checks, analytics/logging,
/// global error handling, or A/B redirect logic.
/// </remarks>
/// <example>
/// <code>
/// public class AuthInterceptor : INavigationInterceptor
/// {
///     public Task OnNavigatingAsync(NavigationContext context)
///     {
///         if (!_auth.IsLoggedIn)
///             throw new OperationCanceledException("Not authenticated.");
///         return Task.CompletedTask;
///     }
///
///     public Task OnNavigatedAsync(NavigationContext context) => Task.CompletedTask;
/// }
///
/// // Registration:
/// services.RegisterNavigationInterceptor&lt;AuthInterceptor&gt;();
/// </code>
/// </example>
public interface INavigationInterceptor
{
    /// <summary>
    /// Called before navigation begins (before the pipeline runs).
    /// Throw <see cref="OperationCanceledException"/> to abort the navigation.
    /// </summary>
    Task OnNavigatingAsync(NavigationContext context);

    /// <summary>
    /// Called after navigation completes successfully.
    /// Exceptions thrown here are logged but do not affect the navigation result.
    /// </summary>
    Task OnNavigatedAsync(NavigationContext context);
}
