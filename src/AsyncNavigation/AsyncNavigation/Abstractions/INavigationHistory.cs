namespace AsyncNavigation.Abstractions;

public interface INavigationHistory
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
