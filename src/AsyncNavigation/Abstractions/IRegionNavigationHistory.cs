namespace AsyncNavigation.Abstractions;

internal interface IRegionNavigationHistory
{
    bool CanGoBack { get; }
    bool CanGoForward { get; }
    IReadOnlyList<NavigationContext> History { get; }
    NavigationContext? Current { get; }
    void Add(NavigationContext context);
    NavigationContext? GoBack();
    NavigationContext? GoForward();
    void Clear();
}
