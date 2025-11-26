namespace AsyncNavigation.Abstractions;

public interface IRegionFactory
{
    /// <summary>
    /// Registers a region adapter with the specified priority.
    /// </summary>
    /// <param name="adapter">The region adapter to register. Cannot be <see langword="null"/>.</param>
    void RegisterAdapter(IRegionAdapter adapter);
    IRegion CreateRegion(string name,
        object control,
        IServiceProvider serviceProvider,
        bool? useCache = null);
}
