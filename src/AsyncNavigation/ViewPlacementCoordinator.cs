using AsyncNavigation.Abstractions;
using System.Collections.Concurrent;

namespace AsyncNavigation;

/// <summary>Coordinates navigation and window placement by region and view instance.</summary>
public sealed class ViewPlacementCoordinator
{
    private readonly ConcurrentDictionary<string, RegionGate> _gates = new();
    private readonly ConcurrentDictionary<IView, Func<CancellationToken, Task>> _floating = new(ReferenceEqualityComparer.Instance);

    /// <summary>
    /// Acquires the placement lease for a region. Reentrant within the same logical async
    /// call chain: code that already holds the lease (e.g. a navigation callback that in turn
    /// triggers a placement change on the same region) re-enters immediately instead of
    /// deadlocking against itself.
    /// </summary>
    /// <remarks>
    /// Deliberately not an `async` method: AsyncLocal writes made inside an async method are
    /// invisible to its caller once it returns (the compiler-generated state machine restores
    /// the caller's captured ExecutionContext), even when the method never actually suspends.
    /// The uncontended/reentrant fast paths below write <see cref="RegionGate.Depth"/> directly
    /// in this plain method body so the mark stays visible to the caller's subsequent code -
    /// which is what lets a nested re-entry on the same region see it. Only the genuinely
    /// contended wait is delegated to an async helper, where that guarantee no longer holds.
    /// </remarks>
    public Task<IDisposable> EnterAsync(string regionName, CancellationToken cancellationToken = default)
    {
        var gate = _gates.GetOrAdd(regionName, _ => new RegionGate());
        if (gate.Depth.Value > 0)
        {
            gate.Depth.Value++;
            return Task.FromResult<IDisposable>(new Lease(gate, reentrant: true));
        }

        var wait = gate.Semaphore.WaitAsync(cancellationToken);
        if (wait.IsCompletedSuccessfully)
        {
            gate.Depth.Value = 1;
            return Task.FromResult<IDisposable>(new Lease(gate, reentrant: false));
        }
        return EnterSlowAsync(gate, wait);
    }

    private static async Task<IDisposable> EnterSlowAsync(RegionGate gate, Task wait)
    {
        await wait;
        gate.Depth.Value = 1;
        return new Lease(gate, reentrant: false);
    }

    /// <summary>Returns whether the current logical async call chain already holds the placement lease for the region.</summary>
    public bool IsHeld(string regionName) =>
        _gates.TryGetValue(regionName, out var gate) && gate.Depth.Value > 0;

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
        public readonly AsyncLocal<int> Depth = new();
    }

    private sealed class Lease(RegionGate gate, bool reentrant) : IDisposable
    {
        private RegionGate? _gate = gate;
        public void Dispose()
        {
            var g = Interlocked.Exchange(ref _gate, null);
            if (g is null) return;
            if (reentrant)
                g.Depth.Value--;
            else
            {
                g.Depth.Value = 0;
                g.Semaphore.Release();
            }
        }
    }
}
