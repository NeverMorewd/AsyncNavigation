using AsyncNavigation.Core;

namespace AsyncNavigation.Abstractions;

public interface IRegionNavigationService<in T> where T : IRegionProcessor
{
    void Setup(T region);
    Task RequestNavigateAsync(NavigationContext navigationContext);
    Task WaitNavigationAsync(NavigationContext navigationContext);
    IObservable<NavigationContext?> Navigated { get; }
    IObservable<NavigationContext?> Navigating { get; }
    IObservable<NavigationContext?> NavigationFailed { get; }
}
