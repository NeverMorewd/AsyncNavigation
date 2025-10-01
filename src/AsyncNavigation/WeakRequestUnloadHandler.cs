using AsyncNavigation.Abstractions;
using AsyncNavigation.Core;

namespace AsyncNavigation;

internal sealed class WeakRequestUnloadHandler
{
    private readonly IRegionPresenter _regionPresenter;
    private readonly IViewManager _viewCacheManager;
    private readonly Action<INavigationAware> _unloadCallBack;


    public WeakRequestUnloadHandler(
        IRegionPresenter regionPresenter,
        IViewManager viewCacheManager,
        Action<INavigationAware> unloadCallBack)
    {
        _regionPresenter = regionPresenter;
        _viewCacheManager = viewCacheManager;
        _unloadCallBack = unloadCallBack;
    }
    public void Attach(INavigationAware aware, NavigationContext context)
    {
        var weakAware = new WeakReference<INavigationAware>(aware);
        async Task OnRequestUnloadAsync(object sender, AsyncEventArgs args)
        {
            if (!weakAware.TryGetTarget(out var target))
            {
                aware.RequestUnloadAsync -= OnRequestUnloadAsync;
                return;
            }

            try
            {
                _unloadCallBack?.Invoke(target);
                await target.OnUnloadAsync(args.CancellationToken);
                _regionPresenter.ProcessDeactivate(context);
            }
            catch (OperationCanceledException) when (args.CancellationToken.IsCancellationRequested)
            {
                
            }
        }
        aware.RequestUnloadAsync += OnRequestUnloadAsync;
    }

    public void AttachOld(INavigationAware aware, NavigationContext context)
    {
        var weakAware = new WeakReference<INavigationAware>(aware);
        AsyncEventHandler<AsyncEventArgs> handler = null!;

        handler = async (s, e) =>
        {
            if (weakAware.TryGetTarget(out var target))
            {
                try
                {
                    _unloadCallBack?.Invoke(target);
                    await target.OnUnloadAsync(e.CancellationToken);
                }
                catch (OperationCanceledException) when (e.CancellationToken.IsCancellationRequested)
                {
                    return;
                }

                _regionPresenter.ProcessDeactivate(context);
            }
            else
            {
                aware.RequestUnloadAsync -= handler;
            }
        };
        aware.RequestUnloadAsync += handler;
    }
}
