namespace AsyncNavigation.Core;

public class AsyncEventArgs(CancellationToken token) : EventArgs
{
    public static new readonly AsyncEventArgs Empty = new(CancellationToken.None);
    public CancellationToken CancellationToken { get; } = token;
}
