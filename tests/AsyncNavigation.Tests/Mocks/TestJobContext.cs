using AsyncNavigation.Abstractions;

namespace AsyncNavigation.Tests.Mocks;

internal class TestJobContext : IJobContext, IDisposable
{
    public Guid JobId { get; } = Guid.NewGuid();
    public CancellationToken CancellationToken { get; set; } = CancellationToken.None;

    private CancellationTokenSource? _linkedCts;

    public bool Started { get; private set; }
    public bool Completed { get; private set; }

    public void OnStarted() => Started = true;
    public void OnCompleted() => Completed = true;

    public void LinkCancellationToken(CancellationToken otherToken)
    {
        _linkedCts = CancellationTokenSource.CreateLinkedTokenSource(otherToken);
        CancellationToken = _linkedCts.Token;
    }

    public void Dispose() => _linkedCts?.Dispose();
}
