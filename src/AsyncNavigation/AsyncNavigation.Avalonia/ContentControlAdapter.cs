using AsyncNavigation.Abstractions;
using Avalonia.Controls;

namespace AsyncNavigation.Avalonia;

public class ContentControlAdapter : RegionAdapterBase<ContentControl>
{
    public override IRegion CreateRegion(string name, ContentControl control, IServiceProvider serviceProvider, bool? useCache = null)
    {
        return new ContentRegion(name, control, serviceProvider, useCache);
    }
}
