using System.Windows;

namespace AsyncNavigation.Wpf;

public class IndicatorDataTemplate
{
    private readonly Func<NavigationContext, FrameworkElement> _builder;
    public IndicatorDataTemplate(Func<NavigationContext, FrameworkElement> builder)
    {
        _builder = builder;
    }

    public FrameworkElement Build(NavigationContext context)
    {
        return _builder.Invoke(context);
    }
}
