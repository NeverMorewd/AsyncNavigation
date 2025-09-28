namespace AsyncNavigation.Core;

public sealed class SingleAssignment<T> where T : class
{
    private T? _value;
    private int _isSet;

    public T? Value
    {
        get
        {
            if (Volatile.Read(ref _isSet) == 0)
                throw new InvalidOperationException("Value not set yet.");
            return Volatile.Read(ref _value);
        }
        set
        {
            if (Interlocked.CompareExchange(ref _isSet, 1, 0) != 0)
                throw new InvalidOperationException("Value can only be set once.");
            Volatile.Write(ref _value, value);
        }
    }

    public bool IsSet => Volatile.Read(ref _isSet) != 0;

    public bool TrySet(T value)
    {
        if (Interlocked.CompareExchange(ref _isSet, 1, 0) == 0)
        {
            Volatile.Write(ref _value, value);
            return true;
        }
        return false;
    }
}

