using AsyncNavigation.Abstractions;
using AsyncNavigation.Core;
using System.Threading.Tasks;

namespace AsyncNavigation;

internal sealed class RequestUnloadHandler
{
    private readonly Dictionary<INavigationAware, AsyncEventHandler<EventArgs>> _handlers = [];
    private readonly IRegionPresenter _regionPresenter;
    private readonly IViewCacheManager _viewCacheManager;

    public RequestUnloadHandler(IRegionPresenter regionPresenter, IViewCacheManager viewCacheManager)
    {
        _regionPresenter = regionPresenter;
        _viewCacheManager = viewCacheManager;
    }

    public void Attach(INavigationAware aware, NavigationContext context)
    {
        if (_handlers.ContainsKey(aware))
            return;

        Task handler(object s, EventArgs e) => OnRequestUnloadAsync(s, e, context);

        aware.RequestUnloadAsync += handler;
        _handlers[aware] = handler;
    }

    public void Detach(INavigationAware aware)
    {
        if (_handlers.TryGetValue(aware, out var handler))
        {
            aware.RequestUnloadAsync -= handler;
            _handlers.Remove(aware);
        }
    }

  
    public void Clear()
    {
        foreach (var kvp in _handlers.ToList())
        {
            kvp.Key.RequestUnloadAsync -= kvp.Value;
        }
        _handlers.Clear();
    }

    
    private Task OnRequestUnloadAsync(object sender, EventArgs e, NavigationContext context)
    {
        _regionPresenter.ProcessDeactivate(context);
        if  (sender is INavigationAware navigationAware)
        {
            if (navigationAware is IDisposable disposable)
            {
                try
                {
                    disposable.Dispose();
                }
                catch
                {
                    // ignored
                }
            }
        }
        try
        {
            _viewCacheManager.Remove(context.ViewName);
        }
        catch
        {
            // ignored
        }
        return Task.CompletedTask;
    }
}
