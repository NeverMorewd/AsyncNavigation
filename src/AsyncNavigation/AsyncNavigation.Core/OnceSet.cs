namespace AsyncNavigation.Core;

public class OnceSet<T>
{
    private T? _value;
    private int _isSet;

    public T? Value
    {
        get => _value;
        set
        {
            if (Interlocked.CompareExchange(ref _isSet, 1, 0) != 0)
                throw new InvalidOperationException("Value can only be set once.");

            _value = value;
        }
    }

    public bool IsSet => _isSet != 0;
}
