using AsyncNavigation.Core;

namespace AsyncNavigation.Abstractions;

public interface IRegionNavigationService<in T> where T : IRegionProcessor
{
    void SeRegionProcessor(T region);
    Task<NavigationResult> RequestNavigateAsync(NavigationContext navigationContext);
    //Task WaitNavigationAsync(NavigationContext navigationContext);
    //IObservable<NavigationContext?> Navigated { get; }
    //IObservable<NavigationContext?> Navigating { get; }
    //IObservable<NavigationContext?> NavigationFailed { get; }
}
