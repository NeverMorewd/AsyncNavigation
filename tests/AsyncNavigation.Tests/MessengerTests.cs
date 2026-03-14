using AsyncNavigation.Abstractions;
using AsyncNavigation.Tests.Infrastructure;
using AsyncNavigation.Tests.Utils;
using Microsoft.Extensions.DependencyInjection;

namespace AsyncNavigation.Tests;

[Collection(NavigationTestCollection.Name)]
public class MessengerTests
{
    private readonly IMessenger _messenger;

    public MessengerTests(ServiceFixture fixture)
    {
        _messenger = fixture.ServiceProvider.GetRequiredService<IMessenger>();
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    private sealed class PingMessage(string value)
    {
        public string Value { get; } = value;
    }

    private sealed class PongMessage(int count)
    {
        public int Count { get; } = count;
    }

    private sealed class Recipient
    {
        public List<string> Received { get; } = [];
        public List<Task> AsyncTasks { get; } = [];
    }

    // ── Closure-style Subscribe / Send ───────────────────────────────────────

    [Fact]
    public void Send_ClosureStyle_ShouldDeliverMessage()
    {
        var r = new Recipient();
        _messenger.Subscribe<PingMessage>(r, msg => r.Received.Add(msg.Value));

        _messenger.Send(new PingMessage("hello"));

        Assert.Contains("hello", r.Received);
        _messenger.Unsubscribe(r);
    }

    [Fact]
    public void Send_MultipleRecipients_AllShouldReceive()
    {
        var r1 = new Recipient();
        var r2 = new Recipient();
        _messenger.Subscribe<PingMessage>(r1, msg => r1.Received.Add(msg.Value));
        _messenger.Subscribe<PingMessage>(r2, msg => r2.Received.Add(msg.Value));

        _messenger.Send(new PingMessage("broadcast"));

        Assert.Contains("broadcast", r1.Received);
        Assert.Contains("broadcast", r2.Received);
        _messenger.Unsubscribe(r1);
        _messenger.Unsubscribe(r2);
    }

    [Fact]
    public void Send_DifferentMessageType_ShouldNotDeliver()
    {
        var r = new Recipient();
        _messenger.Subscribe<PingMessage>(r, msg => r.Received.Add(msg.Value));

        // PongMessage — r is not subscribed to it
        _messenger.Send(new PongMessage(42));

        Assert.Empty(r.Received);
        _messenger.Unsubscribe(r);
    }

    [Fact]
    public void Send_NoSubscribers_ShouldNotThrow()
    {
        var ex = Record.Exception(() => _messenger.Send(new PingMessage("orphan")));
        Assert.Null(ex);
    }

    // ── Static-style Subscribe ────────────────────────────────────────────────

    [Fact]
    public void Send_StaticStyle_ShouldPassRecipientToHandler()
    {
        var r = new Recipient();
        _messenger.Subscribe<Recipient, PingMessage>(r, static (recipient, msg) => recipient.Received.Add(msg.Value));

        _messenger.Send(new PingMessage("static"));

        Assert.Contains("static", r.Received);
        _messenger.Unsubscribe(r);
    }

    [Fact]
    public async Task Send_StaticStyle_CollectedRecipient_ShouldBePruned()
    {
        var weak = SubscribeWithWeakRecipient();

        // Use the project's GcUtils to reliably wait for collection.
        var collected = await GcUtils.WaitForCollectedAsync(weak, timeoutMs: 3000);
        if (!collected)
        {
            Assert.Fail("Recipient was not collected in time — possible strong reference retained.");
            return;
        }

        // Send after GC: should not throw and the dead subscription is pruned silently.
        var ex = Record.Exception(() => _messenger.Send(new PingMessage("after-gc")));
        Assert.Null(ex);
        Assert.False(weak.IsAlive);
    }

    // Isolated into a separate method so the local `r` is guaranteed out of scope
    // before the GC loop in the test above.
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    private WeakReference SubscribeWithWeakRecipient()
    {
        var r = new Recipient();
        _messenger.Subscribe<Recipient, PingMessage>(r, static (recipient, msg) => recipient.Received.Add(msg.Value));
        return new WeakReference(r);
    }

    // ── Async Subscribe / SendAsync ──────────────────────────────────────────

    [Fact]
    public async Task SendAsync_AsyncHandler_ShouldAwaitAllHandlers()
    {
        var r1 = new Recipient();
        var r2 = new Recipient();
        _messenger.Subscribe<Recipient, PingMessage>(r1, static async (recipient, msg, ct) =>
        {
            await Task.Delay(10, ct);
            recipient.Received.Add(msg.Value + "-r1");
        });
        _messenger.Subscribe<Recipient, PingMessage>(r2, static async (recipient, msg, ct) =>
        {
            await Task.Delay(10, ct);
            recipient.Received.Add(msg.Value + "-r2");
        });

        await _messenger.SendAsync(new PingMessage("async"));

        Assert.Contains("async-r1", r1.Received);
        Assert.Contains("async-r2", r2.Received);
        _messenger.Unsubscribe(r1);
        _messenger.Unsubscribe(r2);
    }

    [Fact]
    public async Task SendAsync_CancellationToken_ShouldPropagate()
    {
        var r = new Recipient();
        _messenger.Subscribe<Recipient, PingMessage>(r, static async (_, _, ct) =>
        {
            await Task.Delay(Timeout.Infinite, ct);
        });

        using var cts = new CancellationTokenSource();
        cts.Cancel();

        // TaskCanceledException is a subclass of OperationCanceledException.
        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => _messenger.SendAsync(new PingMessage("cancelled"), cts.Token));

        _messenger.Unsubscribe(r);
    }

    // ── Unsubscribe ──────────────────────────────────────────────────────────

    [Fact]
    public void Unsubscribe_SpecificType_ShouldStopDelivery()
    {
        var r = new Recipient();
        _messenger.Subscribe<PingMessage>(r, msg => r.Received.Add(msg.Value));

        _messenger.Unsubscribe<PingMessage>(r);
        _messenger.Send(new PingMessage("after-unsub"));

        Assert.Empty(r.Received);
    }

    [Fact]
    public void Unsubscribe_All_ShouldStopAllDelivery()
    {
        var r = new Recipient();
        _messenger.Subscribe<PingMessage>(r, msg => r.Received.Add(msg.Value));
        _messenger.Subscribe<PongMessage>(r, msg => r.Received.Add(msg.Count.ToString()));

        _messenger.Unsubscribe(r);
        _messenger.Send(new PingMessage("x"));
        _messenger.Send(new PongMessage(99));

        Assert.Empty(r.Received);
    }

    [Fact]
    public void Unsubscribe_SpecificType_LeavesOtherTypesIntact()
    {
        var r = new Recipient();
        _messenger.Subscribe<PingMessage>(r, msg => r.Received.Add("ping:" + msg.Value));
        _messenger.Subscribe<PongMessage>(r, msg => r.Received.Add("pong:" + msg.Count));

        _messenger.Unsubscribe<PingMessage>(r);
        _messenger.Send(new PingMessage("gone"));
        _messenger.Send(new PongMessage(7));

        Assert.DoesNotContain("ping:gone", r.Received);
        Assert.Contains("pong:7", r.Received);
        _messenger.Unsubscribe(r);
    }

    // ── Multiple subscriptions from same recipient ────────────────────────────

    [Fact]
    public void Subscribe_SameRecipientTwice_BothHandlersCalled()
    {
        var r = new Recipient();
        _messenger.Subscribe<PingMessage>(r, msg => r.Received.Add("first"));
        _messenger.Subscribe<PingMessage>(r, msg => r.Received.Add("second"));

        _messenger.Send(new PingMessage("dup"));

        Assert.Contains("first", r.Received);
        Assert.Contains("second", r.Received);
        _messenger.Unsubscribe(r);
    }
}
