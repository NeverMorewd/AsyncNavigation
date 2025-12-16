namespace AsyncNavigation.Core;

public class AsyncEventArgs(CancellationToken token) : EventArgs
{
    public new static readonly AsyncEventArgs Empty = new(CancellationToken.None);
    public CancellationToken CancellationToken { get; } = token;
}
