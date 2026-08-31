namespace AsyncNavigation.Floating;

public interface IViewPlacementService
{
    IReadOnlyCollection<IFloatingViewSession> FloatingViews { get; }
    Task<IFloatingViewSession> FloatAsync(string regionName, Guid? navigationId = null,
        FloatingWindowOptions? options = null, CancellationToken cancellationToken = default);
    Task RestoreAsync(Guid sessionId, CancellationToken cancellationToken = default);
    bool TryGetSession(Guid sessionId, out IFloatingViewSession? session);
}
