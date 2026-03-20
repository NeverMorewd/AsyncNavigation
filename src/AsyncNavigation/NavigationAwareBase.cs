using AsyncNavigation.Abstractions;
using AsyncNavigation.Core;

namespace AsyncNavigation;

/// <summary>
/// Abstract base class that provides no-op default implementations of all
/// <see cref="INavigationAware"/> members. Derive from this class when you only
/// need to override a subset of the navigation lifecycle methods, avoiding the
/// boilerplate of empty implementations for the rest.
/// </summary>
/// <example>
/// <code>
/// public class HomeViewModel : NavigationAwareBase
/// {
///     public override async Task OnNavigatedToAsync(NavigationContext context)
///     {
///         // Only override what you need
///         await LoadDataAsync();
///     }
/// }
/// </code>
/// </example>
public abstract class NavigationAwareBase : INavigationAware
{
    /// <inheritdoc/>
    /// <remarks>Called only the first time a view is created and shown. Default implementation does nothing.</remarks>
    public virtual Task InitializeAsync(NavigationContext context) => Task.CompletedTask;

    /// <inheritdoc/>
    /// <remarks>Called every time the view becomes the active view. Default implementation does nothing.</remarks>
    public virtual Task OnNavigatedToAsync(NavigationContext context) => Task.CompletedTask;

    /// <inheritdoc/>
    /// <remarks>Called when navigating away from this view. Default implementation does nothing.</remarks>
    public virtual Task OnNavigatedFromAsync(NavigationContext context) => Task.CompletedTask;

    /// <inheritdoc/>
    /// <remarks>
    /// Controls whether a cached view instance can be reused for the incoming navigation request.
    /// Default returns <see langword="true"/>, meaning the cached instance is always reused.
    /// Override and return <see langword="false"/> to force creation of a new instance.
    /// </remarks>
    public virtual Task<bool> IsNavigationTargetAsync(NavigationContext context) => Task.FromResult(true);

    /// <inheritdoc/>
    /// <remarks>Called when the view is being removed from the region cache. Default implementation does nothing.</remarks>
    public virtual Task OnUnloadAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    /// <inheritdoc/>
    /// <remarks>
    /// Raise this event to request that the framework proactively removes this view from the region.
    /// Use <see cref="RequestUnloadAsync"/> as a convenient helper to raise it.
    /// </remarks>
    public event AsyncEventHandler<AsyncEventArgs>? AsyncRequestUnloadEvent;

    /// <summary>
    /// Raises <see cref="AsyncRequestUnloadEvent"/> to request that the framework remove this view.
    /// </summary>
    protected Task RequestUnloadAsync(CancellationToken cancellationToken = default)
    {
        var handler = AsyncRequestUnloadEvent;
        if (handler is not null)
            return handler(this, new AsyncEventArgs(cancellationToken));
        return Task.CompletedTask;
    }
}
