using AsyncNavigation.Abstractions;
using System.Collections.Concurrent;
using System.Diagnostics;

namespace AsyncNavigation.Floating;

internal sealed class ViewPlacementService : IViewPlacementService, IDisposable
{
    private readonly IRegionManager _regionManager;
    private readonly IFloatingWindowHostFactory _windowFactory;
    private readonly ConcurrentDictionary<Guid, FloatingViewSession> _sessions = [];
    private readonly ConcurrentDictionary<Guid, Guid> _sessionsByNavigationId = [];

    public ViewPlacementService(IRegionManager regionManager, IFloatingWindowHostFactory windowFactory)
    {
        _regionManager = regionManager;
        _windowFactory = windowFactory;
    }

    public IReadOnlyCollection<IFloatingViewSession> FloatingViews =>
        _sessions.Values.Cast<IFloatingViewSession>().ToArray();

    public async Task<IFloatingViewSession> FloatAsync(
        string regionName,
        Guid? navigationId = null,
        FloatingWindowOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(regionName);
        cancellationToken.ThrowIfCancellationRequested();

        if (navigationId.HasValue &&
            _sessionsByNavigationId.TryGetValue(navigationId.Value, out var activeSessionId) &&
            _sessions.TryGetValue(activeSessionId, out var activeSession))
        {
            await activeSession.ActivateAsync(cancellationToken);
            return activeSession;
        }

        if (!_regionManager.TryGetRegion(regionName, out var region))
            throw new InvalidOperationException($"Region '{regionName}' was not found.");
        if (region is not IRegionPlacementParticipant participant)
            throw new NotSupportedException($"Region '{regionName}' does not support view placement changes.");

        var item = participant.Capture(navigationId);
        if (_sessionsByNavigationId.TryGetValue(item.Context.NavigationId, out var existingId) &&
            _sessions.TryGetValue(existingId, out var existing))
        {
            await existing.ActivateAsync(cancellationToken);
            return existing;
        }

        options = (options ?? new FloatingWindowOptions()).WithDefaultTitle(item.Context.ViewName);
        var host = _windowFactory.Create(options);
        var session = new FloatingViewSession(this, Guid.NewGuid(), regionName, item, host);

        if (!_sessionsByNavigationId.TryAdd(item.Context.NavigationId, session.Id))
        {
            await host.DisposeAsync();
            if (_sessionsByNavigationId.TryGetValue(item.Context.NavigationId, out var concurrentId) &&
                _sessions.TryGetValue(concurrentId, out var concurrentSession))
            {
                await concurrentSession.ActivateAsync(cancellationToken);
                return concurrentSession;
            }
            throw new InvalidOperationException("Another floating operation is already in progress for this view.");
        }
        if (!_sessions.TryAdd(session.Id, session))
        {
            _sessionsByNavigationId.TryRemove(item.Context.NavigationId, out _);
            await host.DisposeAsync();
            throw new InvalidOperationException("Could not create a floating view session.");
        }

        host.RestoreRequested += session.OnRestoreRequested;
        var detached = false;
        try
        {
            participant.Detach(item);
            detached = true;
            await host.SetContentAsync(GetContentHost(item), cancellationToken);
            await host.ShowAsync(cancellationToken);
            return session;
        }
        catch
        {
            host.RestoreRequested -= session.OnRestoreRequested;
            _sessions.TryRemove(session.Id, out _);
            _sessionsByNavigationId.TryRemove(item.Context.NavigationId, out _);
            try
            {
                await host.SetContentAsync(null, CancellationToken.None);
                if (detached)
                    participant.Attach(item);
            }
            finally
            {
                await host.DisposeAsync();
            }
            throw;
        }
    }

    public Task RestoreAsync(Guid sessionId, CancellationToken cancellationToken = default)
    {
        if (!_sessions.TryGetValue(sessionId, out var session))
            throw new KeyNotFoundException($"Floating view session '{sessionId}' was not found.");
        return session.RestoreAsync(cancellationToken);
    }

    public bool TryGetSession(Guid sessionId, out IFloatingViewSession? session)
    {
        var found = _sessions.TryGetValue(sessionId, out var value);
        session = value;
        return found;
    }

    internal async Task RestoreCoreAsync(FloatingViewSession session, CancellationToken cancellationToken)
    {
        if (!_regionManager.TryGetRegion(session.OriginRegionName, out var region) ||
            region is not IRegionPlacementParticipant participant)
        {
            throw new InvalidOperationException($"Origin region '{session.OriginRegionName}' is not available.");
        }

        await session.Host.SetContentAsync(null, cancellationToken);
        try
        {
            participant.Attach(session.Item);
        }
        catch
        {
            await session.Host.SetContentAsync(GetContentHost(session.Item), CancellationToken.None);
            throw;
        }

        _sessions.TryRemove(session.Id, out _);
        _sessionsByNavigationId.TryRemove(session.NavigationId, out _);
        session.Host.RestoreRequested -= session.OnRestoreRequested;
        try
        {
            await session.Host.CloseAsync(CancellationToken.None);
            await session.Host.DisposeAsync();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Could not close floating window for '{session.NavigationId}': {ex}");
        }
    }

    public void Dispose()
    {
        foreach (var session in _sessions.Values)
            session.Host.RestoreRequested -= session.OnRestoreRequested;
        _sessions.Clear();
        _sessionsByNavigationId.Clear();
    }

    private static object GetContentHost(RegionPlacementItem item) =>
        item.Context.IndicatorHost.Value?.Host
        ?? throw new InvalidOperationException("The navigation item does not have an indicator host.");

    internal sealed class FloatingViewSession : IFloatingViewSession
    {
        private readonly ViewPlacementService _owner;
        private readonly SemaphoreSlim _gate = new(1, 1);

        internal FloatingViewSession(ViewPlacementService owner, Guid id, string originRegionName,
            RegionPlacementItem item, IFloatingWindowHost host)
        {
            _owner = owner;
            Id = id;
            OriginRegionName = originRegionName;
            Item = item;
            Host = host;
        }

        public Guid Id { get; }
        public Guid NavigationId => Item.Context.NavigationId;
        public string OriginRegionName { get; }
        public ViewPlacementState State { get; private set; } = ViewPlacementState.Floating;
        internal RegionPlacementItem Item { get; }
        internal IFloatingWindowHost Host { get; }

        public async Task RestoreAsync(CancellationToken cancellationToken = default)
        {
            await _gate.WaitAsync(cancellationToken);
            try
            {
                if (State == ViewPlacementState.Restored)
                    return;
                State = ViewPlacementState.Restoring;
                try
                {
                    await _owner.RestoreCoreAsync(this, cancellationToken);
                    State = ViewPlacementState.Restored;
                }
                catch
                {
                    State = ViewPlacementState.Floating;
                    throw;
                }
            }
            finally
            {
                _gate.Release();
            }
        }

        public Task ActivateAsync(CancellationToken cancellationToken = default) =>
            Host.ActivateAsync(cancellationToken);

        internal async void OnRestoreRequested(object? sender, EventArgs e)
        {
            try
            {
                await RestoreAsync();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Could not restore floating view '{NavigationId}': {ex}");
            }
        }
    }
}
