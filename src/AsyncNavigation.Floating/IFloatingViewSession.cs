namespace AsyncNavigation.Floating;

public interface IFloatingViewSession
{
    Guid Id { get; }
    Guid NavigationId { get; }
    string OriginRegionName { get; }
    ViewPlacementState State { get; }
    Task RestoreAsync(CancellationToken cancellationToken = default);
    Task ActivateAsync(CancellationToken cancellationToken = default);
}
