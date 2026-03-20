namespace AsyncNavigation.Core;

internal sealed class WeakAsyncEventHandler<TEventArgs> where TEventArgs : EventArgs
{
    private readonly WeakReference<object> _targetRef;
    private readonly AsyncEventHandler<TEventArgs> _handler;
    private readonly Action<AsyncEventHandler<TEventArgs>> _unsubscribe;

    public WeakAsyncEventHandler(
        object target,
        AsyncEventHandler<TEventArgs> handler,
        Action<AsyncEventHandler<TEventArgs>> unsubscribe)
    {
        _targetRef = new WeakReference<object>(target);
        _handler = handler;
        _unsubscribe = unsubscribe;
    }

    public async Task InvokeAsync(object sender, TEventArgs args)
    {
        // Obtain a strong reference first to avoid the TOCTOU race where the target
        // could be collected between the liveness check and the handler invocation.
        if (_targetRef.TryGetTarget(out _))
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
