using AsyncNavigation.Abstractions;
using AsyncNavigation.Core;

namespace AsyncNavigation;

internal sealed class RequestUnloadHandler
{
    private readonly Dictionary<INavigationAware, AsyncEventHandler<AsyncEventArgs>> _handlers = [];
    private readonly IRegionPresenter _regionPresenter;
    private readonly IViewManager _viewCacheManager;
    private readonly Action<INavigationAware> _unloadCallBack;

    public RequestUnloadHandler(IRegionPresenter regionPresenter, 
        IViewManager viewCacheManager,
        Action<INavigationAware> unloadCallBack)
    {
        _regionPresenter = regionPresenter;
        _viewCacheManager = viewCacheManager;
        _unloadCallBack = unloadCallBack;
    }

    public void Attach(INavigationAware aware, NavigationContext context)
    {
        if (_handlers.ContainsKey(aware))
            return;

        Task handler(object s, AsyncEventArgs e) => OnRequestUnloadAsync(s, e, context);

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

    
    private async Task OnRequestUnloadAsync(object sender, AsyncEventArgs e, NavigationContext context)
    {
        if (sender is INavigationAware aware)
        {
            try
            {
                _unloadCallBack.Invoke(aware);
                Detach(aware);
                await aware.OnUnloadAsync(e.CancellationToken);
            }
            catch (OperationCanceledException) when (e.CancellationToken.IsCancellationRequested)
            {
                Attach(aware, context);
                return;
            }
            _regionPresenter.ProcessDeactivate(context);
            _viewCacheManager.Remove(context.ViewName, true);
        }
    }
}
