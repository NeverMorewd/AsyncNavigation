using AsyncNavigation.Core;

namespace AsyncNavigation;


/// <summary>
/// Defines configuration options for the navigation framework.
/// </summary>
public class NavigationOptions
{
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
    /// This property is obsolete. Use <see cref="MaxHistoryItems"/> instead. 
    /// MaxHistoryItems represents the maximum number of navigation history items globally,
    /// while MaxCachedViews only controlled per-region view caching in the old design.
    /// </remarks>
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
    /// only overwriting properties that differ from the default values.
    /// </summary>
    /// <param name="other">The options to merge from. Can be <c>null</c>.</param>
    public void MergeFrom(NavigationOptions other)
    {
        if (other == null) return;

        if (other.MaxCachedViews != Default.MaxCachedViews)
            MaxCachedViews = other.MaxCachedViews;

        if (other.MaxHistoryItems != Default.MaxHistoryItems)
            MaxHistoryItems = other.MaxHistoryItems;

        if (other.LoadingIndicatorDelay != Default.LoadingIndicatorDelay)
            LoadingIndicatorDelay = other.LoadingIndicatorDelay;

        if (other.NavigationJobStrategy != Default.NavigationJobStrategy)
            NavigationJobStrategy = other.NavigationJobStrategy;

        if (other.NavigationJobScope != Default.NavigationJobScope)
            NavigationJobScope = other.NavigationJobScope;

        if (other.MaxReplayItems != Default.MaxReplayItems)
            MaxReplayItems = other.MaxReplayItems;
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
