using AsyncNavigation.Core;

namespace AsyncNavigation;


/// <summary>
/// Defines configuration options for the navigation framework.
/// </summary>
public class NavigationOptions
{
    // Factory defaults used by MergeFrom to detect user-overridden values.
    // Kept in sync with the property initializers below.
    internal const int DefaultMaxCachedViews = 10;
    internal const int DefaultMaxHistoryItems = 10;
    internal const int DefaultMaxReplayItems = 10;
    internal static readonly TimeSpan DefaultLoadingIndicatorDelay = TimeSpan.FromMilliseconds(100);
    internal const NavigationJobStrategy DefaultNavigationJobStrategy = NavigationJobStrategy.CancelCurrent;
    internal const NavigationJobScope DefaultNavigationJobScope = NavigationJobScope.Region;

    private int _loadingIndicatorRegistered;
    private int _errorIndicatorRegistered;

    /// <summary>
    /// Gets the global default navigation options instance.
    /// </summary>
    public static NavigationOptions Default { get; } = new();

    /// <summary>
    /// Gets or sets the maximum number of cached views in the navigation system.
    /// </summary>
    /// <remarks>
    /// This property is obsolete. Use <see cref="MaxHistoryItems"/> for history size
    /// or configure caching per-region via the PreferCache attached property.
    /// </remarks>
    [Obsolete("MaxCachedViews is obsolete. Use MaxHistoryItems for history size or configure caching per-region via the PreferCache attached property.")]
    public int MaxCachedViews { get; set; } = 10;

    /// <summary>
    /// Gets or sets the maximum number of navigation history items globally.
    /// </summary>
    public int MaxHistoryItems { get; set; } = 10;

    /// <summary>
    /// Gets or sets the maximum number of navigation replay items globally.
    /// </summary>
    public int MaxReplayItems { get; set; } = 10;


    /// <summary>
    /// Gets or sets the delay before showing a loading indicator during navigation.
    /// </summary>
    /// <remarks>
    /// Default value is <c>100 milliseconds</c>.  
    /// This avoids flashing the indicator for very fast navigations.
    /// </remarks>
    public TimeSpan LoadingIndicatorDelay { get; set; } = TimeSpan.FromMilliseconds(100);

    /// <summary>
    /// Gets a value indicating whether the loading indicator feature is enabled.
    /// </summary>
    /// <remarks>
    /// True if a loading indicator has been registered via <see cref="EnsureSingleLoadingIndicator"/>.
    /// </remarks>
    public bool EnableLoadingIndicator => _loadingIndicatorRegistered == 1;

    /// <summary>
    /// Gets a value indicating whether the error indicator feature is enabled.
    /// </summary>
    /// <remarks>
    /// True if an error indicator has been registered via <see cref="EnsureSingleErrorIndicator"/>.
    /// </remarks>
    public bool EnableErrorIndicator => _errorIndicatorRegistered == 1;

    /// <summary>
    /// Gets or sets the navigation job execution strategy.
    /// </summary>
    /// <remarks>
    /// Determines how new navigation requests are handled when another navigation is in progress.
    /// Default value is <see cref="NavigationJobStrategy.CancelCurrent"/>.
    /// </remarks>
    public NavigationJobStrategy NavigationJobStrategy { get; set; } = NavigationJobStrategy.CancelCurrent;

    /// <summary>
    /// Gets or sets the scope in which navigation jobs are managed.
    /// </summary>
    /// <remarks>
    /// Determines whether navigation task lifetime is application or scoped to a specific region.
    /// Default value is <see cref="NavigationJobScope.Region"/>.
    /// </remarks>
    public NavigationJobScope NavigationJobScope { get; set; } = NavigationJobScope.Region;


    public ViewCacheStrategy ViewCacheStrategy { get; set; } = ViewCacheStrategy.IgnoreDuplicateKey;

    /// <summary>
    /// Ensures that only one loading indicator can be registered in the navigation system.
    /// </summary>
    /// <exception cref="NavigationException">Thrown if a loading indicator is already registered.</exception>
    internal void EnsureSingleLoadingIndicator()
    {
        if (Interlocked.Exchange(ref _loadingIndicatorRegistered, 1) == 1)
            throw new NavigationException("Only one loading indicator can be registered.");
    }

    /// <summary>
    /// Ensures that only one error indicator can be registered in the navigation system.
    /// </summary>
    /// <exception cref="NavigationException">Thrown if an error indicator is already registered.</exception>
    internal void EnsureSingleErrorIndicator()
    {
        if (Interlocked.Exchange(ref _errorIndicatorRegistered, 1) == 1)
            throw new NavigationException("Only one error indicator can be registered.");
    }


    /// <summary>
    /// Merges the properties of another <see cref="NavigationOptions"/> instance into this instance,
    /// only overwriting properties that differ from the factory default values.
    /// </summary>
    /// <remarks>
    /// Compares each property on <paramref name="other"/> against the known factory defaults
    /// (not against <c>Default</c>) so the comparison is stable even when called on the
    /// <c>Default</c> instance itself.
    /// </remarks>
    /// <param name="other">The options to merge from. Can be <c>null</c>.</param>
    public void MergeFrom(NavigationOptions other)
    {
        if (other == null) return;

#pragma warning disable CS0618 // MaxCachedViews is obsolete but MergeFrom must handle it for backwards compatibility
        if (other.MaxCachedViews != DefaultMaxCachedViews)
            MaxCachedViews = other.MaxCachedViews;
#pragma warning restore CS0618

        if (other.MaxHistoryItems != DefaultMaxHistoryItems)
            MaxHistoryItems = other.MaxHistoryItems;

        if (other.LoadingIndicatorDelay != DefaultLoadingIndicatorDelay)
            LoadingIndicatorDelay = other.LoadingIndicatorDelay;

        if (other.NavigationJobStrategy != DefaultNavigationJobStrategy)
            NavigationJobStrategy = other.NavigationJobStrategy;

        if (other.NavigationJobScope != DefaultNavigationJobScope)
            NavigationJobScope = other.NavigationJobScope;

        if (other.MaxReplayItems != DefaultMaxReplayItems)
            MaxReplayItems = other.MaxReplayItems;

        if (other.ViewCacheStrategy != ViewCacheStrategy.IgnoreDuplicateKey)
            ViewCacheStrategy = other.ViewCacheStrategy;
    }

    public override bool Equals(object? obj)
    {
        if (obj is NavigationOptions navigationOptions)
        {
            return MaxCachedViews == navigationOptions.MaxCachedViews &&
                   MaxHistoryItems == navigationOptions.MaxHistoryItems &&
                   MaxReplayItems == navigationOptions.MaxReplayItems &&
                   LoadingIndicatorDelay == navigationOptions.LoadingIndicatorDelay &&
                   NavigationJobStrategy == navigationOptions.NavigationJobStrategy &&
                   NavigationJobScope == navigationOptions.NavigationJobScope;
        }
        return base.Equals(obj);
    }
    public override int GetHashCode()
    {
        return HashCode.Combine(
            MaxCachedViews,
            MaxHistoryItems,
            MaxReplayItems,
            LoadingIndicatorDelay,
            NavigationJobStrategy,
            NavigationJobScope);
    }
}
