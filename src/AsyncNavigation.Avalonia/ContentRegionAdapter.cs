using AsyncNavigation.Abstractions;
using Avalonia.Controls;

namespace AsyncNavigation.Avalonia;

public class ContentRegionAdapter : RegionAdapterBase<ContentControl>
{
    public override IRegion CreateRegion(string name, 
        ContentControl control, 
        IServiceProvider serviceProvider, 
        bool? useCache)
    {
        return new ContentRegion(name, control, serviceProvider, useCache);
    }
}
