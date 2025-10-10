namespace AsyncNavigation.Abstractions;

internal interface IJobContext
{
    Guid JobId { get; }
    CancellationToken CancellationToken { get; }
    void OnStarted();
    void OnCompleted();
    void LinkCancellationToken(CancellationToken otherToken);
}
