namespace AsyncNavigation.Abstractions;

public interface IRegionAdapter<T> : IRegionAdapter
{
    bool IsAdapted(T control);
    IRegion CreateRegion(T control, IServiceProvider serviceProvider, bool? useCache = null);
}
