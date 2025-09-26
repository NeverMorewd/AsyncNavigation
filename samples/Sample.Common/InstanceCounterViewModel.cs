namespace Sample.Common;

public abstract class InstanceCounterViewModel<T> : ViewModelBase
    where T : InstanceCounterViewModel<T>
{
    private static int _instanceCounter = 0;

    protected InstanceCounterViewModel()
    {
        InstanceNumber = Interlocked.Increment(ref _instanceCounter);
    }

    public int InstanceNumber { get; }
}
