using AsyncNavigation.Core;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace AsyncNavigation.Wpf;

public class ContentRegion : RegionBase<ContentRegion, ContentControl>
{
    public ContentRegion(string name, 
        ContentControl contentControl, 
        IServiceProvider serviceProvider, 
        bool? useCache) : base(name, contentControl, serviceProvider)
    {
        ArgumentNullException.ThrowIfNull(contentControl);
        ArgumentNullException.ThrowIfNull(serviceProvider);

        RegionControlAccessor.ExecuteOn(control => 
        {
            control.Tag = this;
            control.SetBinding(ContentControl.ContentProperty,
                new Binding(nameof(RegionContext.Selected))
                {
                    Source = _context,
                    Mode = BindingMode.TwoWay
                });
        });

        RegionControlAccessor.ExecuteOn(control =>
        {
            var dataTemplate = new DataTemplate();
            var factory = new FrameworkElementFactory(typeof(ContentPresenter));
            factory.SetBinding(ContentPresenter.ContentProperty,
                new Binding("IndicatorHost.Value.Host")
                {
                    FallbackValue = null
                });
            dataTemplate.VisualTree = factory;

            control.ContentTemplate = dataTemplate;
        });



        EnableViewCache = useCache ?? true;
        IsSinglePageRegion = true;
    }

    public override void Dispose()
    {
        base.Dispose();
        _context.Selected = null;
        RegionControlAccessor.ExecuteOn(control =>
        {
            control.Content = null;
        });
    }

    //public override void RenderIndicator(NavigationContext navigationContext)
    //{
    //   _context.Selected = navigationContext;
    //}

    public override void ProcessActivate(NavigationContext navigationContext)
    {
        _context.Selected = navigationContext;
    }

    public override void ProcessDeactivate(NavigationContext? navigationContext)
    {
        _context.Selected = null;
    }
}
