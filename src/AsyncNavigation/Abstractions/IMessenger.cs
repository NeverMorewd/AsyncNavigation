namespace AsyncNavigation.Abstractions;

/// <summary>
/// Lightweight publish/subscribe messenger for ViewModel-to-ViewModel
/// and View-to-ViewModel communication.
/// </summary>
/// <remarks>
/// Recipients are held via weak references so that forgotten subscriptions do not
/// prevent garbage collection.  There are two subscription styles:
/// <list type="bullet">
///   <item>
///     <b>Closure style</b> — <c>Subscribe&lt;TMessage&gt;(recipient, msg => ...)</c><br/>
///     Convenient, but if the lambda captures <c>this</c> the recipient is kept alive
///     by the delegate.  Always pair with an explicit <see cref="Unsubscribe(object)"/>
///     call (e.g. in <see cref="INavigationAware.OnNavigatedFromAsync"/>).
///   </item>
///   <item>
///     <b>Static style</b> — <c>Subscribe&lt;TRecipient, TMessage&gt;(recipient, static (r, msg) => ...)</c><br/>
///     The handler receives the recipient as an explicit parameter; a <c>static</c>
///     lambda cannot accidentally capture <c>this</c>, so the weak reference is the
///     only thing keeping the subscription alive.  No manual unsubscription needed.
///   </item>
/// </list>
/// </remarks>
public interface IMessenger
{
    // ── Subscribe ────────────────────────────────────────────────────────────

    /// <summary>
    /// Registers a closure-style synchronous handler.
    /// </summary>
    void Subscribe<TMessage>(object recipient, Action<TMessage> handler)
        where TMessage : class;

    /// <summary>
    /// Registers a static-style synchronous handler.
    /// The <paramref name="handler"/> receives the <paramref name="recipient"/> as its first
    /// argument, which enables <c>static</c> lambdas and eliminates accidental captures.
    /// </summary>
    void Subscribe<TRecipient, TMessage>(TRecipient recipient, Action<TRecipient, TMessage> handler)
        where TRecipient : class
        where TMessage : class;

    /// <summary>
    /// Registers a static-style asynchronous handler.
    /// </summary>
    void Subscribe<TRecipient, TMessage>(TRecipient recipient, Func<TRecipient, TMessage, CancellationToken, Task> handler)
        where TRecipient : class
        where TMessage : class;

    // ── Unsubscribe ──────────────────────────────────────────────────────────

    /// <summary>Removes all handlers for <typeparamref name="TMessage"/> registered by <paramref name="recipient"/>.</summary>
    void Unsubscribe<TMessage>(object recipient)
        where TMessage : class;

    /// <summary>Removes every handler registered by <paramref name="recipient"/>, across all message types.</summary>
    void Unsubscribe(object recipient);

    // ── Send ─────────────────────────────────────────────────────────────────

    /// <summary>
    /// Delivers <paramref name="message"/> to all current subscribers synchronously.
    /// Async handlers are started but not awaited.
    /// </summary>
    void Send<TMessage>(TMessage message)
        where TMessage : class;

    /// <summary>
    /// Delivers <paramref name="message"/> to all current subscribers.
    /// Async handlers are awaited sequentially.
    /// </summary>
    Task SendAsync<TMessage>(TMessage message, CancellationToken cancellationToken = default)
        where TMessage : class;
}
