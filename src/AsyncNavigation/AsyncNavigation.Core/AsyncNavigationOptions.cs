namespace AsyncNavigation.Core;


public static class AsyncNavigationOptions
{
    private static int _loadingIndicatorRegistered;
    private static int _errorIndicatorRegistered;
    public static bool EnableViewCaching { get; set; } = true;
    public static int MaxCachedViews { get; set; } = 10;
    public static TimeSpan NavigationTimeout { get; set; } = TimeSpan.FromSeconds(30);
    public static bool ParallelNavigationLifecycle { get; set; } = false;
    public static bool EnableLoadingIndicator { get => _loadingIndicatorRegistered == 1; }
    public static bool EnableErrorIndicator { get => _errorIndicatorRegistered == 1; }

    internal static void EnsureSingleLoadingIndicator()
    {
        if (Interlocked.Exchange(ref _loadingIndicatorRegistered, 1) == 1)
            throw new NavigationException("Only one loading indicator can be registered.");
    }

    internal static void EnsureSingleErrorIndicator()
    {
        if (Interlocked.Exchange(ref _errorIndicatorRegistered, 1) == 1)
            throw new NavigationException("Only one error indicator can be registered.");
    }
}
