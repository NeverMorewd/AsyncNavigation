namespace AsyncNavigation.Abstractions;

public interface IInnerIndicatorProviderBase
{
    bool HasErrorIndicator(NavigationContext context);
    bool HasLoadingIndicator(NavigationContext context);
}

