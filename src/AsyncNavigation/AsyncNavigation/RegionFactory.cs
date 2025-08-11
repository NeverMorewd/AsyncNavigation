using AsyncNavigation.Abstractions;

namespace AsyncNavigation;

public class RegionFactory
{
    private readonly List<IRegionAdapter> _adapters = [];

    public RegionFactory(IEnumerable<IRegionAdapter> adapters)
    {
        _adapters.AddRange(adapters);
    }

    public void RegisterAdapter(IRegionAdapter adapter)
    {
        _adapters.Add(adapter);
    }

    public IRegion CreateRegion(string name, 
        object control, 
        IServiceProvider serviceProvider, 
        bool? useCache = null)
    {
        var adapter = _adapters.FirstOrDefault(a => a.IsAdapted(control));
        return adapter == null? 
            throw new NotSupportedException($"Unsupported control: {control.GetType()}"): 
            adapter.CreateRegion(name, control, serviceProvider, useCache);
    }
}
