namespace AsyncNavigation.Abstractions;

public interface IRegionIndicatorManager
{
    void Setup(NavigationContext context, bool useSingleton);
    Task ShowContentAsync(NavigationContext context, object content);
    Task ShowErrorAsync(NavigationContext context, Exception exception);
    Task StartAsync(NavigationContext context, Task processTask, TimeSpan? delayTime = null);
}
