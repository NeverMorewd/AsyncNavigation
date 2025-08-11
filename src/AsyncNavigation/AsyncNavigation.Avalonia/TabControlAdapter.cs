using AsyncNavigation.Abstractions;

namespace AsyncNavigation.Avalonia;

public class TabControlAdapter : IRegionAdapter
{
    public bool IsAdapted(object control)
    {
        return true;
    }

    public IRegion CreateRegion(string name, object control, IServiceProvider serviceProvider, bool? useCache = null)
    {
        throw new NotImplementedException();
    }
}
