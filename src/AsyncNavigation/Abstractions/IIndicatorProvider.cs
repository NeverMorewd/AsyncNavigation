namespace AsyncNavigation.Abstractions;

public interface IIndicatorProvider
{
    bool HasIndicator();
    IRegionIndicator GetIndicator();
}
