namespace AsyncNavigation.Core;

internal sealed class WeakAsyncEventHandler<TEventArgs> where TEventArgs : EventArgs
{
    private readonly WeakReference _targetRef;
    private readonly AsyncEventHandler<TEventArgs> _handler;
    private readonly Action<AsyncEventHandler<TEventArgs>> _unsubscribe;

    public WeakAsyncEventHandler(
        object target,
        AsyncEventHandler<TEventArgs> handler,
        Action<AsyncEventHandler<TEventArgs>> unsubscribe)
    {
        _targetRef = new WeakReference(target);
        _handler = handler;
        _unsubscribe = unsubscribe;
    }

    public async Task InvokeAsync(object sender, TEventArgs args)
    {
        if (_targetRef.IsAlive)
        {
            await _handler(sender, args);
        }
        else
        {
            _unsubscribe(_handler);
        }
    }

    public AsyncEventHandler<TEventArgs> GetHandlerDelegate() => InvokeAsync;
}
