using AsyncNavigation.Abstractions;

namespace AsyncNavigation.Core;

public record NavigationTarget(string RegionName, 
    string ViewName, 
    INavigationParameters? Parameters = null);
