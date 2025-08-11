using AsyncNavigation.Core;

namespace AsyncNavigation.Abstractions;

public interface IRegionIndicatorManager<T> where T : new()
{
    T SetupIndicator(NavigationContext context);
    T SetupSingletonIndicator(NavigationContext navigationContext);
    Task ShowLoadingAsync(NavigationContext context, CancellationToken cancellationToken = default);
    Task DelayShowLoadingAsync(NavigationContext context, Task processTask, CancellationToken cancellationToken = default);
    Task ShowErrorAsync(NavigationContext context, Exception exception, CancellationToken cancellationToken = default);
    Task ShowContentAsync(NavigationContext context, object content, CancellationToken cancellationToken = default);
}
