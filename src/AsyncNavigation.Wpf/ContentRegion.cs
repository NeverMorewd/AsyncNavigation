using AsyncNavigation.Core;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace AsyncNavigation.Wpf;

public class ContentRegion : RegionBase<ContentRegion>
{
    private readonly ContentControl _contentControl;
    public ContentRegion(ContentControl contentControl, IServiceProvider serviceProvider, bool? useCache) : base(serviceProvider)
    {
        ArgumentNullException.ThrowIfNull(contentControl);
        ArgumentNullException.ThrowIfNull(serviceProvider);
        _contentControl = contentControl;

        _contentControl.SetBinding(ContentControl.ContentProperty,
        new Binding(nameof(RegionContext.Selected))
        {
            Source = _context,
            Mode = BindingMode.TwoWay
        });

        var dataTemplate = new DataTemplate();
        var factory = new FrameworkElementFactory(typeof(ContentPresenter));
        factory.SetBinding(ContentPresenter.ContentProperty,
            new Binding("IndicatorHost.Value.Host") 
            { 
                FallbackValue = null 
            });
        dataTemplate.VisualTree = factory;

        _contentControl.ContentTemplate = dataTemplate;


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
        //_contentControl.Content = navigationContext.IndicatorHost.Value.Host;
    }

    public override void ProcessActivate(NavigationContext navigationContext)
    {
        _context.Selected = navigationContext;
        //_contentControl.Content = navigationContext.IndicatorHost.Value.Host;
    }

    public override void ProcessDeactivate(NavigationContext navigationContext)
    {
        _context.Selected = null;
    }
}
