using AsyncNavigation.Abstractions;
using Avalonia.Controls;
using Microsoft.Extensions.DependencyInjection;

namespace AsyncNavigation.Avalonia;

internal sealed class InnerIndicatorHost : IInnerRegionIndicatorHost, IInnerIndicatorProvider
{
    private readonly ContentControl _host;
    private readonly IInnerIndicatorProvider _innerIndicatorProvider;
    public InnerIndicatorHost(IServiceProvider serviceProvider)
    {
        _host = new ContentControl();
        var innerIndicatorProvider = serviceProvider.GetService<IInnerIndicatorProvider>();
        if (innerIndicatorProvider != null)
        {
            _innerIndicatorProvider = innerIndicatorProvider;
        }
        else
        {
            _innerIndicatorProvider = this;
        }
    }

    public object Host => _host;

    Task IRegionIndicator.OnCancelledAsync(NavigationContext context)
    {
        _host.Content = null;
        return Task.CompletedTask;
    }

    Task IRegionIndicator.OnLoadedAsync(NavigationContext context)
    {
        return Task.CompletedTask;
    }

    Task IInnerRegionIndicatorHost.ShowContentAsync(NavigationContext context)
    {
        _host.Content = context.Target.Value;
        return Task.CompletedTask;
    }

    Task IRegionIndicator.ShowErrorAsync(NavigationContext context, Exception? innerException)
    {
        if (_innerIndicatorProvider.HasErrorIndicator(context))
        {
            _host.Content = _innerIndicatorProvider.GetErrorIndicator(context);
        }
        return Task.CompletedTask;
    }

    Task IRegionIndicator.ShowLoadingAsync(NavigationContext context)
    {
        if (_innerIndicatorProvider.HasLoadingIndicator(context))
        {
            _host.Content = _innerIndicatorProvider.GetLoadingIndicator(context);
        }
        return Task.CompletedTask;
    }

    public Control GetErrorIndicator(NavigationContext navigationContext)
    {
        throw new NotImplementedException();
    }

    public Control GetLoadingIndicator(NavigationContext navigationContext)
    {
        throw new NotImplementedException();
    }

    public bool HasErrorIndicator(NavigationContext navigationContext)
    {
        return false;
    }

    public bool HasLoadingIndicator(NavigationContext navigationContext)
    {
        return false;
    }
}
