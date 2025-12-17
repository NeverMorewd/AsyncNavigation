using AsyncNavigation.Abstractions;
using AsyncNavigation.Core;
using System;
using System.Reactive.Linq;

// ReSharper disable once CheckNamespace
namespace AsyncNavigation;

public static class RegionExtensions
{
    public static IObservable<NavigationEventArgs> NavigatedAsObservable(this IRegion region)
    {
        return Observable.FromEvent<EventHandler<NavigationEventArgs>, NavigationEventArgs>(
            handler => (sender, args) => handler(args),
            h => region.Navigated += h,
            h => region.Navigated -= h);
    }
}
