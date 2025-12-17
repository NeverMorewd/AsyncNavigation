using System.Reactive.Linq;
using AsyncNavigation.Abstractions;
using AsyncNavigation.Core;

// ReSharper disable once CheckNamespace
namespace AsyncNavigation;

public static class RegionManagerExtensions
{
    extension (IRegionManager regionManager)
    {
        public IObservable<RegionChangeEventArgs> RegionChanges =>
            Observable.FromEvent<EventHandler<RegionChangeEventArgs>, RegionChangeEventArgs>(
                h => (_, e) => h(e),
                h => regionManager.RegionChanged += h,
                h => regionManager.RegionChanged -= h);

        public IObservable<NavigationEventArgs> NavigationEvents =>
           regionManager.RegionChanges
               .Where(e => e.RegionChangeKind == RegionChangeKind.Added)
               .Select(e => e.Region.NavigatedAsObservable()
                   .TakeUntil(regionManager.RegionChanges
                       .Where(x =>
                           x.RegionChangeKind == RegionChangeKind.Removed &&
                           x.Region == e.Region)))
               .StartWith(regionManager.Regions.Values.Select(r =>
                       r.NavigatedAsObservable()
                        .TakeUntil(regionManager.RegionChanges
                           .Where(x =>
                               x.RegionChangeKind == RegionChangeKind.Removed &&
                               x.Region == r))))
               .Merge();
    }
}
