namespace AsyncNavigation.Core;

public sealed class EmptyDisposable : IDisposable
{
    /// <summary>
    /// Singleton default disposable.
    /// </summary>
    public static readonly EmptyDisposable Instance = new();

    private EmptyDisposable()
    {
    }

    /// <summary>
    /// Does nothing.
    /// </summary>
    public void Dispose()
    {
        // no op
    }
}
