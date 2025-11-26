namespace AsyncNavigation.Abstractions;

public interface IRegionAdapter
{
    /// <summary>
    /// The priority of the adapter.Higher values indicate higher priority.
    /// </summary>
    uint Priority { get; }
    bool IsAdapted(object control);
    IRegion CreateRegion(string name, object control, IServiceProvider serviceProvider, bool? useCache);
}
