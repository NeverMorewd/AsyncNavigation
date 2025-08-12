using AsyncNavigation.Core;

namespace AsyncNavigation.Abstractions;

public interface IRegionIndicatorManager<T> where T : new()
{
    T SetupIndicator(NavigationContext context);
    T SetupSingletonIndicator(NavigationContext navigationContext);
    Task ShowLoadingAsync(NavigationContext context);
    Task DelayShowLoadingAsync(NavigationContext context, Task processTask);
    Task ShowErrorAsync(NavigationContext context, Exception exception);
    Task ShowContentAsync(NavigationContext context, object content);
}
