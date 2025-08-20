using AsyncNavigation.Abstractions;
using Avalonia.Controls;

namespace AsyncNavigation.Avalonia;

public class ContentRegionAdapter : RegionAdapterBase<ContentControl>
{
    public override IRegion CreateRegion(ContentControl control, 
        IServiceProvider serviceProvider, 
        bool? useCache = null)
    {
        return new ContentRegion(control, serviceProvider, useCache);
    }
}
