namespace AsyncNavigation.Floating;

public interface IFloatingWindowHost : IAsyncDisposable
{
    event EventHandler? RestoreRequested;
    Task SetContentAsync(object? content, CancellationToken cancellationToken = default);
    Task ShowAsync(CancellationToken cancellationToken = default);
    Task ActivateAsync(CancellationToken cancellationToken = default);
    Task CloseAsync(CancellationToken cancellationToken = default);
}

public interface IFloatingWindowHostFactory
{
    IFloatingWindowHost Create(FloatingWindowOptions options);
}
