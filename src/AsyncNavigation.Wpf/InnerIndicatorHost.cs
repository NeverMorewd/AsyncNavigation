using AsyncNavigation.Abstractions;
using System.Windows.Controls;

namespace AsyncNavigation.Wpf;

internal sealed class InnerIndicatorHost : IInnerRegionIndicatorHost
{
    private readonly ContentControl _host;
    private readonly IInnerIndicatorProvider _inlineIndicatorProvider;
    public InnerIndicatorHost(IInnerIndicatorProvider inlineIndicatorProvider)
    {
        _host = new ContentControl();
        _inlineIndicatorProvider = inlineIndicatorProvider;
    }

    public object Host => _host;

    public Task ShowContentAsync(NavigationContext context)
    {
        _host.Content = context.Target.Value;
        return Task.CompletedTask;
    }

    public Task ShowErrorAsync(NavigationContext context, Exception? innerException)
    {
        if (_inlineIndicatorProvider.HasErrorIndicator(context))
        {
            _host.Content = _inlineIndicatorProvider.GetErrorIndicator(context);
        }
        return Task.CompletedTask;
    }

    public Task ShowLoadingAsync(NavigationContext context)
    {
        if (_inlineIndicatorProvider.HasLoadingIndicator(context))
        {
            _host.Content = _inlineIndicatorProvider.GetLoadingIndicator(context);
        }
        return Task.CompletedTask;
    }
}
