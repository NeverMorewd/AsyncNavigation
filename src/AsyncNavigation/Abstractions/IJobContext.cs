namespace AsyncNavigation.Abstractions;

internal interface IJobContext
{
    Guid JobId { get; }
    CancellationToken CancellationToken { get; internal set; }
    void OnStarted();
    void OnCompleted();
}
