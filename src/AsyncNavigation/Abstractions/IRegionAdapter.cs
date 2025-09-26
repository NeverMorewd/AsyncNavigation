namespace AsyncNavigation.Abstractions;

public interface IRegionAdapter
{
    bool IsAdapted(object control);
    IRegion CreateRegion(object control, IServiceProvider serviceProvider, bool? useCache = null);
}
