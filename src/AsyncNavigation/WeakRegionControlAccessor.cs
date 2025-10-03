using AsyncNavigation.Abstractions;
using System.Diagnostics.CodeAnalysis;

namespace AsyncNavigation;

public sealed class WeakRegionControlAccessor<TControl> : IRegionControlAccessor<TControl> where TControl : class
{
    private readonly WeakReference<TControl> _controlRef;
    //private readonly TControl _controlRef;
    private volatile bool _alive = true;
    public WeakRegionControlAccessor(TControl control)
    {
        ArgumentNullException.ThrowIfNull(control);
        _controlRef = new WeakReference<TControl>(control);
        //_controlRef = control;
    }

    public void ExecuteOn(Action<TControl> action)
    {
        action(Ensure());
    }

    public TResult ExecuteOn<TResult>(Func<TControl, TResult> func)
    {
        return func(Ensure());
    }
    public TControl Ensure()
    {
        if (TryGet(out var control) && control is not null)
        {
            return control;
        }
        throw new ObjectDisposedException($"The control of region '{control}' has been disposed.");
    }
    public bool TryGet([MaybeNullWhen(false)] out TControl control)
    {
        if (!_alive)
        {
            control = null;
            return false;
        }
        //control = _controlRef;
        _alive = _controlRef.TryGetTarget(out control);
        //_alive = true;
        return _alive;
    }
}
