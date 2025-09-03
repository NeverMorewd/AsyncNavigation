namespace AsyncNavigation.Abstractions;

public interface IInlineIndicator : IRegionIndicator
{
    object IndicatorControl { get; }
    void ShowLoading(NavigationContext context);
    void ShowError(NavigationContext context, Exception? exception);
    void ShowContent(NavigationContext context);
}
