namespace AsyncNavigation.Core;

public class NavigationOptions
{
    private int _loadingIndicatorRegistered;
    private int _errorIndicatorRegistered;

    public static NavigationOptions Default { get; } = new();

    public int MaxCachedItems { get; set; } = 10;
    public TimeSpan NavigationTimeout { get; set; } = TimeSpan.FromMilliseconds(10000);
    public TimeSpan LoadingIndicatorDelay { get; set; } = TimeSpan.FromMilliseconds(100);
    public bool EnableLoadingIndicator => _loadingIndicatorRegistered == 1;
    public bool EnableErrorIndicator => _errorIndicatorRegistered == 1;
    public NavigationTaskStrategy NavigationTaskStrategy { get; set; } = NavigationTaskStrategy.CancelCurrent;
    public NavigationTaskScope NavigationTaskScope { get; set; } = NavigationTaskScope.Region;


    internal void EnsureSingleLoadingIndicator()
    {
        if (Interlocked.Exchange(ref _loadingIndicatorRegistered, 1) == 1)
            throw new NavigationException("Only one loading indicator can be registered.");
    }

    internal void EnsureSingleErrorIndicator()
    {
        if (Interlocked.Exchange(ref _errorIndicatorRegistered, 1) == 1)
            throw new NavigationException("Only one error indicator can be registered.");
    }

    public void MergeFrom(NavigationOptions other)
    {
        if (other == null) return;

        if (other.MaxCachedItems != Default.MaxCachedItems)
            MaxCachedItems = other.MaxCachedItems;

        if (other.NavigationTimeout != Default.NavigationTimeout)
            NavigationTimeout = other.NavigationTimeout;

        if (other.LoadingIndicatorDelay != Default.LoadingIndicatorDelay)
            LoadingIndicatorDelay = other.LoadingIndicatorDelay;

        if (other.NavigationTaskStrategy != Default.NavigationTaskStrategy)
            NavigationTaskStrategy = other.NavigationTaskStrategy;

        if (other.NavigationTaskScope != Default.NavigationTaskScope)
            NavigationTaskScope = other.NavigationTaskScope;
    }
}
