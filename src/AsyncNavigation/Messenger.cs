using AsyncNavigation.Abstractions;

namespace AsyncNavigation;

/// <inheritdoc cref="IMessenger"/>
internal sealed class Messenger : IMessenger
{
    private readonly object _lock = new();

    // Keyed by exact message type.
    private readonly Dictionary<Type, List<SubscriptionBase>> _subscriptions = [];

    // ── Subscription hierarchy ───────────────────────────────────────────────

    private abstract class SubscriptionBase
    {
        /// <summary>Returns true if <paramref name="recipient"/> is the tracked recipient and is still alive.</summary>
        public abstract bool IsAliveFor(object recipient);

        /// <summary>Invokes the handler synchronously. Returns false if the recipient has been collected.</summary>
        public abstract bool TryInvoke(object message);

        /// <summary>Invokes the handler (awaiting async ones). Returns false if the recipient has been collected.</summary>
        public abstract Task<bool> TryInvokeAsync(object message, CancellationToken cancellationToken);
    }

    /// <summary>Closure-style handler — handler may capture the recipient.</summary>
    private sealed class ClosureSubscription<TMessage>(object recipient, Action<TMessage> handler)
        : SubscriptionBase where TMessage : class
    {
        private readonly WeakReference<object> _ref = new(recipient);

        public override bool IsAliveFor(object recipient)
            => _ref.TryGetTarget(out var r) && ReferenceEquals(r, recipient);

        public override bool TryInvoke(object message)
        {
            if (!_ref.TryGetTarget(out _)) return false;
            handler((TMessage)message);
            return true;
        }

        public override Task<bool> TryInvokeAsync(object message, CancellationToken ct)
        {
            return Task.FromResult(TryInvoke(message));
        }
    }

    /// <summary>Static-style synchronous handler — recipient is passed in explicitly, so the lambda can be <c>static</c>.</summary>
    private sealed class StaticSyncSubscription<TRecipient, TMessage>(
        TRecipient recipient, Action<TRecipient, TMessage> handler)
        : SubscriptionBase
        where TRecipient : class
        where TMessage : class
    {
        private readonly WeakReference<TRecipient> _ref = new(recipient);

        public override bool IsAliveFor(object recipient)
            => _ref.TryGetTarget(out var r) && ReferenceEquals(r, recipient);

        public override bool TryInvoke(object message)
        {
            if (!_ref.TryGetTarget(out var r)) return false;
            handler(r, (TMessage)message);
            return true;
        }

        public override Task<bool> TryInvokeAsync(object message, CancellationToken ct)
            => Task.FromResult(TryInvoke(message));
    }

    /// <summary>Static-style asynchronous handler.</summary>
    private sealed class StaticAsyncSubscription<TRecipient, TMessage>(
        TRecipient recipient, Func<TRecipient, TMessage, CancellationToken, Task> handler)
        : SubscriptionBase
        where TRecipient : class
        where TMessage : class
    {
        private readonly WeakReference<TRecipient> _ref = new(recipient);

        public override bool IsAliveFor(object recipient)
            => _ref.TryGetTarget(out var r) && ReferenceEquals(r, recipient);

        public override bool TryInvoke(object message)
        {
            if (!_ref.TryGetTarget(out var r)) return false;
            // Fire-and-forget when called synchronously.
            _ = handler(r, (TMessage)message, CancellationToken.None);
            return true;
        }

        public override async Task<bool> TryInvokeAsync(object message, CancellationToken ct)
        {
            if (!_ref.TryGetTarget(out var r)) return false;
            await handler(r, (TMessage)message, ct).ConfigureAwait(false);
            return true;
        }
    }

    // ── IMessenger implementation ────────────────────────────────────────────

    public void Subscribe<TMessage>(object recipient, Action<TMessage> handler)
        where TMessage : class
    {
        ArgumentNullException.ThrowIfNull(recipient);
        ArgumentNullException.ThrowIfNull(handler);
        lock (_lock)
            GetOrCreate(typeof(TMessage)).Add(new ClosureSubscription<TMessage>(recipient, handler));
    }

    public void Subscribe<TRecipient, TMessage>(TRecipient recipient, Action<TRecipient, TMessage> handler)
        where TRecipient : class
        where TMessage : class
    {
        ArgumentNullException.ThrowIfNull(recipient);
        ArgumentNullException.ThrowIfNull(handler);
        lock (_lock)
            GetOrCreate(typeof(TMessage)).Add(new StaticSyncSubscription<TRecipient, TMessage>(recipient, handler));
    }

    public void Subscribe<TRecipient, TMessage>(TRecipient recipient, Func<TRecipient, TMessage, CancellationToken, Task> handler)
        where TRecipient : class
        where TMessage : class
    {
        ArgumentNullException.ThrowIfNull(recipient);
        ArgumentNullException.ThrowIfNull(handler);
        lock (_lock)
            GetOrCreate(typeof(TMessage)).Add(new StaticAsyncSubscription<TRecipient, TMessage>(recipient, handler));
    }

    public void Unsubscribe<TMessage>(object recipient)
        where TMessage : class
    {
        lock (_lock)
        {
            if (_subscriptions.TryGetValue(typeof(TMessage), out var list))
                list.RemoveAll(s => s.IsAliveFor(recipient));
        }
    }

    public void Unsubscribe(object recipient)
    {
        lock (_lock)
        {
            foreach (var list in _subscriptions.Values)
                list.RemoveAll(s => s.IsAliveFor(recipient));
        }
    }

    public void Send<TMessage>(TMessage message)
        where TMessage : class
    {
        ArgumentNullException.ThrowIfNull(message);

        List<SubscriptionBase> snapshot;
        lock (_lock)
        {
            if (!_subscriptions.TryGetValue(typeof(TMessage), out var list)) return;
            snapshot = [.. list];
        }

        List<SubscriptionBase>? dead = null;
        foreach (var sub in snapshot)
            if (!sub.TryInvoke(message))
                (dead ??= []).Add(sub);

        if (dead is { Count: > 0 })
            Prune(typeof(TMessage), dead);
    }

    public async Task SendAsync<TMessage>(TMessage message, CancellationToken cancellationToken = default)
        where TMessage : class
    {
        ArgumentNullException.ThrowIfNull(message);

        List<SubscriptionBase> snapshot;
        lock (_lock)
        {
            if (!_subscriptions.TryGetValue(typeof(TMessage), out var list)) return;
            snapshot = [.. list];
        }

        List<SubscriptionBase>? dead = null;
        foreach (var sub in snapshot)
            if (!await sub.TryInvokeAsync(message, cancellationToken).ConfigureAwait(false))
                (dead ??= []).Add(sub);

        if (dead is { Count: > 0 })
            Prune(typeof(TMessage), dead);
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    private List<SubscriptionBase> GetOrCreate(Type messageType)
    {
        if (!_subscriptions.TryGetValue(messageType, out var list))
            _subscriptions[messageType] = list = [];
        return list;
    }

    private void Prune(Type messageType, List<SubscriptionBase> dead)
    {
        lock (_lock)
        {
            if (_subscriptions.TryGetValue(messageType, out var list))
                foreach (var d in dead)
                    list.Remove(d);
        }
    }
}
