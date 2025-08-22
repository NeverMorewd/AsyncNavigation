namespace AsyncNavigation.Core;

public class AsyncEventArgs : EventArgs
{
    public static new readonly AsyncEventArgs Empty = new(CancellationToken.None);
    public CancellationToken CancellationToken { get; }
    public AsyncEventArgs(CancellationToken token) => CancellationToken = token;
}
