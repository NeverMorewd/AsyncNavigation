using AsyncNavigation.Abstractions;
using System.Windows.Controls;

namespace AsyncNavigation.Wpf;

public class ContentRegionAdapter : RegionAdapterBase<ContentControl>
{
    public override IRegion CreateRegion(ContentControl control, 
        IServiceProvider serviceProvider, 
        bool? useCache = null)
    {
        return new ContentRegion(control, serviceProvider, useCache);
    }
}
