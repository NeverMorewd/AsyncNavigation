using AsyncNavigation.Abstractions;
using System.Collections.Concurrent;

namespace AsyncNavigation;

/// <summary>Coordinates navigation and window placement by region and view instance.</summary>
public sealed class ViewPlacementCoordinator
{
    private readonly ConcurrentDictionary<string, RegionGate> _gates = new();
    private readonly ConcurrentDictionary<IView, Func<CancellationToken, Task>> _floating = new(ReferenceEqualityComparer.Instance);

    // Identifies "this logical operation" so a nested EnterAsync call on the same region (e.g. a
    // navigation callback that triggers a placement change) can recognize it already holds the
    // lease instead of deadlocking against itself. Must be established by EnsureFlow() - called
    // synchronously, before any await - at every top-level entry point (RequestNavigateAsync,
    // GoBackAsync/GoForwardAsync, FloatAsync, RestoreCoreAsync, CloseCoreAsync): an AsyncLocal
    // write made inside a called async method never becomes visible to that method's own caller
    // once it returns (the compiler-generated state machine restores the caller's captured
    // ExecutionContext on resume), even when the method never truly suspends. Establishing the id
    // in the entry point's own frame sidesteps that entirely - the value then flows forward
    // through every await *it* performs and everything it calls, which is the direction
    // AsyncLocal always propagates correctly.
    private readonly AsyncLocal<Guid> _flowId = new();

    /// <summary>
    /// Marks the start of one logical placement operation so nested <see cref="EnterAsync"/>
    /// calls on the same region can be recognized as reentrant. A no-op when already nested
    /// inside an ancestor's flow (the ambient id is left alone). Callers should wrap their whole
    /// operation: <c>using var flow = coordinator.EnsureFlow(); using var lease = await
    /// coordinator.EnterAsync(...);</c>.
    /// </summary>
    public IDisposable EnsureFlow()
    {
        if (_flowId.Value != Guid.Empty)
            return NoopScope.Instance;
        _flowId.Value = Guid.NewGuid();
        return new FlowScope(this);
    }

    /// <summary>
    /// Acquires the placement lease for a region. Reentrant within the same <see cref="EnsureFlow"/>
    /// scope: code that already holds the lease re-enters immediately instead of deadlocking
    /// against itself.
    /// </summary>
    public Task<IDisposable> EnterAsync(string regionName, CancellationToken cancellationToken = default)
    {
        var gate = _gates.GetOrAdd(regionName, _ => new RegionGate());
        var flowId = _flowId.Value;
        if (flowId != Guid.Empty && gate.HolderFlowId == flowId)
            return Task.FromResult<IDisposable>(new Lease(gate, reentrant: true));

        var wait = gate.Semaphore.WaitAsync(cancellationToken);
        if (wait.IsCompletedSuccessfully)
        {
            gate.HolderFlowId = flowId;
            return Task.FromResult<IDisposable>(new Lease(gate, reentrant: false));
        }
        return EnterSlowAsync(gate, wait, flowId);
    }

    private static async Task<IDisposable> EnterSlowAsync(RegionGate gate, Task wait, Guid flowId)
    {
        await wait;
        // A plain field write on the shared gate, not an AsyncLocal - ordinary object state isn't
        // subject to the ExecutionContext isolation described above, so it's fine that this runs
        // inside an async method the caller merely awaits.
        gate.HolderFlowId = flowId;
        return new Lease(gate, reentrant: false);
    }

    /// <summary>Returns whether the current EnsureFlow scope already holds the placement lease for the region.</summary>
    public bool IsHeld(string regionName)
    {
        var flowId = _flowId.Value;
        return flowId != Guid.Empty && _gates.TryGetValue(regionName, out var gate) && gate.HolderFlowId == flowId;
    }

    public bool IsFloating(IView view) => _floating.ContainsKey(view);

    public void Register(IView view, Func<CancellationToken, Task> activate)
    {
        if (!_floating.TryAdd(view, activate))
            throw new InvalidOperationException("The view instance is already floating.");
    }

    public void Unregister(IView view) => _floating.TryRemove(view, out _);

    public async Task<bool> TryActivateAsync(IView view, CancellationToken cancellationToken)
    {
        if (!_floating.TryGetValue(view, out var activate)) return false;
        await activate(cancellationToken);
        return true;
    }

    private sealed class RegionGate
    {
        public readonly SemaphoreSlim Semaphore = new(1, 1);
        public Guid HolderFlowId;
    }

    private sealed class Lease(RegionGate gate, bool reentrant) : IDisposable
    {
        private RegionGate? _gate = gate;
        public void Dispose()
        {
            if (reentrant)
            {
                _gate = null;
                return;
            }
            var g = Interlocked.Exchange(ref _gate, null);
            if (g is null) return;
            g.HolderFlowId = Guid.Empty;
            g.Semaphore.Release();
        }
    }

    private sealed class NoopScope : IDisposable
    {
        public static readonly NoopScope Instance = new();
        public void Dispose() { }
    }

    private sealed class FlowScope(ViewPlacementCoordinator owner) : IDisposable
    {
        public void Dispose() => owner._flowId.Value = Guid.Empty;
    }
}
