namespace AsyncNavigation.Floating;

public interface IFloatingViewSession
{
    Guid Id { get; }
    Guid NavigationId { get; }
    string OriginRegionName { get; }
    ViewPlacementState State { get; }
    event EventHandler? StateChanged;
    Task CloseAsync(CancellationToken cancellationToken = default);
    Task RestoreAsync(CancellationToken cancellationToken = default);
    Task ActivateAsync(CancellationToken cancellationToken = default);
}
