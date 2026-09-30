using AsyncNavigation.Abstractions;
using System.Collections.Concurrent;
using System.Diagnostics;

namespace AsyncNavigation.Floating;

internal sealed class ViewPlacementService : IViewPlacementService, IDisposable
{
    private readonly ViewPlacementCoordinator _placement;
    private readonly IRegionManager _regionManager;
    private readonly IFloatingWindowHostFactory _windowFactory;
    private readonly ConcurrentDictionary<Guid, FloatingViewSession> _sessions = [];
    private readonly ConcurrentDictionary<Guid, Guid> _sessionsByNavigationId = [];

    public ViewPlacementService(IRegionManager regionManager, IFloatingWindowHostFactory windowFactory, ViewPlacementCoordinator placement)
    {
        _placement = placement;
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
        using var flow = _placement.EnsureFlow();
        using var lease = await _placement.EnterAsync(regionName, cancellationToken);

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
        var contentOwner = item.Context.IndicatorHost.Value as IRegionPlacementContentHost;
        var session = new FloatingViewSession(this, Guid.NewGuid(), regionName, region, item, contentOwner, host);

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
        host.CloseRequested += session.OnCloseRequested;
        var registered = false;
        var detached = false;
        var contentDetached = false;
        try
        {
            if (item.Context.Target.Value is { } view)
            {
                _placement.Register(view, session.ActivateAsync);
                registered = true;
            }
            var content = contentOwner?.DetachContent() ?? GetContentHost(item);
            contentDetached = contentOwner is not null;
            session.SetContent(content);
            participant.Detach(item);
            detached = true;
            await host.SetContentAsync(content, cancellationToken);
            await host.ShowAsync(cancellationToken);
            (region as IRegionPlacementNavigation)?.OnViewDetached(item);
            return session;
        }
        catch
        {
            if (registered && item.Context.Target.Value is { } view) _placement.Unregister(view);
            host.RestoreRequested -= session.OnRestoreRequested;
            host.CloseRequested -= session.OnCloseRequested;
            _sessions.TryRemove(session.Id, out _);
            _sessionsByNavigationId.TryRemove(item.Context.NavigationId, out _);
            try
            {
                await host.SetContentAsync(null, CancellationToken.None);
                if (detached)
                    participant.Attach(item, activate: false);
                if (contentDetached)
                    contentOwner!.AttachContent(session.Content);
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
        using var flow = _placement.EnsureFlow();
        using var lease = await _placement.EnterAsync(session.OriginRegionName, cancellationToken);
        if (!session.OriginRegion.TryGetTarget(out var originalRegion) ||
            !_regionManager.TryGetRegion(session.OriginRegionName, out var region) ||
            region is not IRegionPlacementParticipant participant || !ReferenceEquals(region, originalRegion))
        {
            throw new InvalidOperationException($"Origin region '{session.OriginRegionName}' is not available.");
        }

        var contentAttached = false;
        async Task TransferContent()
        {
            await session.Host.SetContentAsync(null, cancellationToken);
            session.ContentOwner?.AttachContent(session.Content);
            contentAttached = true;
        }
        async Task RollbackContent()
        {
            if (contentAttached && session.ContentOwner is not null)
                session.ContentOwner.DetachContent();
            await session.Host.SetContentAsync(session.Content, CancellationToken.None);
        }

        if (region is IRegionPlacementNavigation navigation)
        {
            await navigation.RestorePlacementAsync(session.Item, TransferContent, RollbackContent, cancellationToken);
        }
        else
        {
            var attached = false;
            try
            {
                await TransferContent();
                participant.Attach(session.Item, activate: false);
                attached = true;
            }
            catch
            {
                if (attached) participant.Detach(session.Item);
                await RollbackContent();
                throw;
            }
        }

        if (session.Item.Context.Target.Value is { } restoredView) _placement.Unregister(restoredView);
        _sessions.TryRemove(session.Id, out _);
        _sessionsByNavigationId.TryRemove(session.NavigationId, out _);
        session.Host.RestoreRequested -= session.OnRestoreRequested;
        session.Host.CloseRequested -= session.OnCloseRequested;
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

    internal async Task CloseCoreAsync(FloatingViewSession session, CancellationToken cancellationToken)
    {
        using var flow = _placement.EnsureFlow();
        using var lease = await _placement.EnterAsync(session.OriginRegionName, cancellationToken);
        await session.Host.SetContentAsync(null, cancellationToken);
        try
        {
            await session.Host.CloseAsync(CancellationToken.None);
        }
        catch
        {
            await session.Host.SetContentAsync(session.Content, CancellationToken.None);
            throw;
        }

        if (session.Item.Context.Target.Value is { } closedView)
        {
            _placement.Unregister(closedView);
            if (session.OriginRegion.TryGetTarget(out var origin))
                (origin as IRegionPlacementNavigation)?.OnViewClosed(session.Item);
            DisposeClosedView(closedView);
        }
        _sessions.TryRemove(session.Id, out _);
        _sessionsByNavigationId.TryRemove(session.NavigationId, out _);
        session.Host.RestoreRequested -= session.OnRestoreRequested;
        session.Host.CloseRequested -= session.OnCloseRequested;
        try
        {
            await session.Host.DisposeAsync();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Could not dispose floating window for '{session.NavigationId}': {ex}");
        }
    }

    private static void DisposeClosedView(IView view)
    {
        foreach (var value in new object?[] { view, view.DataContext }.Distinct(ReferenceEqualityComparer.Instance))
        {
            try { (value as IDisposable)?.Dispose(); }
            catch (Exception ex) { Debug.WriteLine($"Could not dispose closed view: {ex}"); }
        }
    }

    public void Dispose()
    {
        foreach (var session in _sessions.Values)
        {
            if (session.Item.Context.Target.Value is { } view) _placement.Unregister(view);
            session.Host.RestoreRequested -= session.OnRestoreRequested;
            session.Host.CloseRequested -= session.OnCloseRequested;
        }
        _sessions.Clear();
        _sessionsByNavigationId.Clear();
    }

    private static object GetContentHost(RegionPlacementItem item)
    {
        if (item.Context.IndicatorHost.Value is not { } host)
            throw new InvalidOperationException("The navigation item does not have an indicator host.");
        // Reaching here means the host isn't an IRegionPlacementContentHost, so its content
        // cannot be detached from its current visual parent. Fail fast instead of proceeding
        // as if detachment succeeded, which would corrupt the visual tree.
        throw new NotSupportedException(
            $"Indicator host '{host.GetType()}' does not implement {nameof(IRegionPlacementContentHost)} " +
            "and cannot be detached for floating placement.");
    }

    internal sealed class FloatingViewSession : IFloatingViewSession
    {
        private readonly ViewPlacementService _owner;
        private readonly SemaphoreSlim _gate = new(1, 1);

        internal FloatingViewSession(ViewPlacementService owner, Guid id, string originRegionName, IRegion originRegion,
            RegionPlacementItem item, IRegionPlacementContentHost? contentOwner, IFloatingWindowHost host)
        {
            _owner = owner;
            Id = id;
            OriginRegionName = originRegionName;
            OriginRegion = new WeakReference<IRegion>(originRegion);
            Item = item;
            ContentOwner = contentOwner;
            Host = host;
        }

        public Guid Id { get; }
        public Guid NavigationId => Item.Context.NavigationId;
        public string OriginRegionName { get; }

        private ViewPlacementState _state = ViewPlacementState.Floating;
        public ViewPlacementState State
        {
            get => _state;
            private set
            {
                if (_state == value) return;
                _state = value;
                StateChanged?.Invoke(this, EventArgs.Empty);
            }
        }
        public event EventHandler? StateChanged;
        internal WeakReference<IRegion> OriginRegion { get; }
        internal RegionPlacementItem Item { get; }
        internal object Content { get; private set; } = null!;
        internal IRegionPlacementContentHost? ContentOwner { get; }
        internal IFloatingWindowHost Host { get; }

        internal void SetContent(object content)
        {
            if (Content is not null)
                throw new InvalidOperationException("Floating session content has already been assigned.");
            Content = content;
        }

        public async Task RestoreAsync(CancellationToken cancellationToken = default)
        {
            await _gate.WaitAsync(cancellationToken);
            try
            {
                if (State is ViewPlacementState.Restored or ViewPlacementState.Closed)
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

        public async Task CloseAsync(CancellationToken cancellationToken = default)
        {
            await _gate.WaitAsync(cancellationToken);
            try
            {
                if (State is ViewPlacementState.Restored or ViewPlacementState.Closed)
                    return;
                State = ViewPlacementState.Closing;
                try
                {
                    await _owner.CloseCoreAsync(this, cancellationToken);
                    State = ViewPlacementState.Closed;
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

        internal async void OnCloseRequested(object? sender, EventArgs e)
        {
            try
            {
                await CloseAsync();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Could not close floating view '{NavigationId}': {ex}");
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
