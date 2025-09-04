namespace AsyncNavigation.Abstractions;

public interface IInnerRegionIndicatorHost : IRegionIndicator
{
    object Host { get; }
    Task ShowContentAsync(NavigationContext context);
}
