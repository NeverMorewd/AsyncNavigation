using AsyncNavigation.Abstractions;
using Avalonia.Controls;

namespace AsyncNavigation.Avalonia;

public class ContentControlAdapter : IRegionAdapter
{
    public bool IsAdapted(object control)
    {
        return control is ContentControl;
    }

    public IRegion CreateRegion(string name, object control, IServiceProvider serviceProvider)
    {
        if (control is ContentControl contentControl)
        {
            return new ContentRegion(name, contentControl, serviceProvider);
        }
        throw new InvalidOperationException($"{control.GetType()} does not match {typeof(ContentControl)}");
    }
}
