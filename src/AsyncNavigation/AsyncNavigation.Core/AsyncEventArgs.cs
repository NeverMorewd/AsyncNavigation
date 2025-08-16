namespace AsyncNavigation.Core;

public class AsyncEventArgs : EventArgs
{
    public CancellationToken CancellationToken { get; }
    public AsyncEventArgs(CancellationToken token) => CancellationToken = token;
}
