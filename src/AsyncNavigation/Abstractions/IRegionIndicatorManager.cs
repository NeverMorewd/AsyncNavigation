namespace AsyncNavigation.Abstractions;

public interface IRegionIndicatorManager
{
    void Setup(NavigationContext context, bool useSingleton);
    Task ShowErrorAsync(NavigationContext context, Exception exception, bool throwIfNeed);
    Task StartAsync(NavigationContext context, Task processTask, TimeSpan? delayTime = null);
}
