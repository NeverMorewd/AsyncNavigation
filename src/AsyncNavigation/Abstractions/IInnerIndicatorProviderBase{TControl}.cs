namespace AsyncNavigation.Abstractions;

public interface IInnerIndicatorProviderBase<TControl>
{
    bool HasErrorIndicator(NavigationContext navigationContext);
    bool HasLoadingIndicator(NavigationContext navigationContext);
    TControl GetErrorIndicator(NavigationContext navigationContext);
    TControl GetLoadingIndicator(NavigationContext navigationContext);
}

