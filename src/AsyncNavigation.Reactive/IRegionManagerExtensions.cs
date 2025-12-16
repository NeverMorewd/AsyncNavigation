using System.Reactive.Linq;
using AsyncNavigation.Abstractions;
using AsyncNavigation.Core;

namespace AsyncNavigation.Reactive;

public static class IRegionManagerExtensions
{
    extension (IRegionManager regionManager)
    {
        public IObservable<NavigationEventArgs> NavigationEvents  =>
            regionManager.Regions.Values
                .Select(region =>
                    Observable.FromEvent<EventHandler<NavigationEventArgs>, NavigationEventArgs>(
                        h => (_, e) => h(e),
                        h => region.Navigated += h,
                        h => region.Navigated -= h))
                .Merge();
    }

}
