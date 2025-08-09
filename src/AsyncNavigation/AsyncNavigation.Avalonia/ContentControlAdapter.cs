using AsyncNavigation.Abstractions;
using Avalonia.Controls;

namespace AsyncNavigation.Avalonia;

public class ContentControlAdapter : RegionAdapterBase<ContentControl>
{
    public override IRegion CreateRegion(string name, ContentControl control, IServiceProvider serviceProvider)
    {
        return new ContentRegion(name, control, serviceProvider);
    }
}
