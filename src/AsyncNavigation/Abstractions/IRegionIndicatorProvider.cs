namespace AsyncNavigation.Abstractions;

public interface IRegionIndicatorProvider
{
    bool HasIndicator(string regionName);
    IRegionIndicator GetIndicator(string regionName);
}