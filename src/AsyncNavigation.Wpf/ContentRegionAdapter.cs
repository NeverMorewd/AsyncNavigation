using AsyncNavigation.Abstractions;
using System.Windows.Controls;

namespace AsyncNavigation.Wpf;

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
