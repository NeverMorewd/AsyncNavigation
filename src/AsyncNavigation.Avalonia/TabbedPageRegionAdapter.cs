using AsyncNavigation.Abstractions;
using Avalonia.Controls;

namespace AsyncNavigation.Avalonia;

public class TabbedPageRegionAdapter : RegionAdapterBase<TabbedPage>
{
    public override IRegion CreateRegion(string name, TabbedPage control, IServiceProvider serviceProvider, bool? useCache)
    {
        return new TabbedPageRegion(name, control,serviceProvider,useCache);
    }
}
