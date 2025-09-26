using AsyncNavigation.Core;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Data;

namespace AsyncNavigation.Avalonia;

public class ContentRegion : RegionBase<ContentRegion>
{
    private readonly ContentControl _contentControl;
    public ContentRegion(string name, 
        ContentControl contentControl, 
        IServiceProvider serviceProvider, 
        bool? useCache) : base(name, serviceProvider)
    {
        ArgumentNullException.ThrowIfNull(contentControl);
        ArgumentNullException.ThrowIfNull(serviceProvider);
        _contentControl = contentControl;

        _contentControl.ContentTemplate = new FuncDataTemplate<NavigationContext>((context, np) =>
        {
            return context?.IndicatorHost.Value?.Host as Control;
        });


        _contentControl.Bind(
            ContentControl.ContentProperty,
            new Binding(nameof(RegionContext.Selected)) { Source = _context, Mode = BindingMode.TwoWay });


        EnableViewCache = useCache ?? true;
        IsSinglePageRegion = true;
    }

    public override void Dispose()
    {
        base.Dispose();
        _contentControl.Content = null;
        _context.Selected = null;
    }

    public override void RenderIndicator(NavigationContext navigationContext)
    {
        _context.Selected = navigationContext;
    }

    public override void ProcessActivate(NavigationContext navigationContext)
    {
        _context.Selected = navigationContext;
    }

    public override void ProcessDeactivate(NavigationContext navigationContext)
    {
        _context.Selected = null;
    }
}
