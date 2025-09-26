namespace AsyncNavigation.Abstractions;

public interface IRegionFactory
{
    void RegisterAdapter(IRegionAdapter adapter);
    IRegion CreateRegion(string name,
        object control,
        IServiceProvider serviceProvider,
        bool? useCache = null);
}
