namespace AsyncNavigation.Abstractions;

public interface IRegionAdapter
{
    bool IsAdapted(object control);
    IRegion CreateRegion(string name, object control, IServiceProvider serviceProvider);
}
