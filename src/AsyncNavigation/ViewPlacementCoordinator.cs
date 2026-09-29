using AsyncNavigation.Abstractions;
using System.Collections.Concurrent;

namespace AsyncNavigation;

/// <summary>Coordinates navigation and window placement by region and view instance.</summary>
public sealed class ViewPlacementCoordinator
{
    private readonly ConcurrentDictionary<string, SemaphoreSlim> _gates = new();
    private readonly ConcurrentDictionary<IView, Func<CancellationToken, Task>> _floating = new(ReferenceEqualityComparer.Instance);

    public async Task<IDisposable> EnterAsync(string regionName, CancellationToken cancellationToken = default)
    {
        var gate = _gates.GetOrAdd(regionName, _ => new SemaphoreSlim(1, 1));
        await gate.WaitAsync(cancellationToken);
        return new Lease(gate);
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

    private sealed class Lease(SemaphoreSlim gate) : IDisposable
    {
        private SemaphoreSlim? _gate = gate;
        public void Dispose() => Interlocked.Exchange(ref _gate, null)?.Release();
    }
}
