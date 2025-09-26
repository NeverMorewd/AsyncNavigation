using AsyncNavigation.Core;

namespace AsyncNavigation.Abstractions;

public interface IRegionIndicator
{
    Task ShowErrorAsync(NavigationContext context, Exception? innerException = null);
    Task ShowLoadingAsync(NavigationContext context);
    Task OnLoadedAsync(NavigationContext context);
    Task OnCancelledAsync(NavigationContext context);
}
