using AsyncNavigation.Abstractions;
using AsyncNavigation.Core;
using AsyncNavigation.Floating;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace AsyncNavigation.Tests;

public sealed class ViewPlacementServiceTests
{
    [Fact]
    public async Task FloatAndRestore_MovesTheSameRenderedHostWithoutNavigating()
    {
        var fixture = new Fixture();

        var session = await fixture.Service.FloatAsync("main", fixture.Context.NavigationId);

        Assert.Equal(1, fixture.Region.DetachCount);
        Assert.Same(fixture.RenderedHost, fixture.Window.Content);
        Assert.True(fixture.Window.WasShown);
        Assert.Single(fixture.Service.FloatingViews);

        await session.RestoreAsync();

        Assert.Equal(1, fixture.Region.AttachCount);
        Assert.Null(fixture.Window.Content);
        Assert.True(fixture.Window.WasClosed);
        Assert.Empty(fixture.Service.FloatingViews);
        Assert.Equal(ViewPlacementState.Restored, session.State);
    }

    [Fact]
    public async Task FloatWithSameNavigationId_ActivatesExistingSession()
    {
        var fixture = new Fixture();

        var first = await fixture.Service.FloatAsync("main", fixture.Context.NavigationId);
        var second = await fixture.Service.FloatAsync("main", fixture.Context.NavigationId);

        Assert.Same(first, second);
        Assert.Equal(1, fixture.Window.ActivateCount);
        Assert.Equal(1, fixture.Region.CaptureCount);
        Assert.Equal(1, fixture.Region.DetachCount);
    }

    [Fact]
    public async Task FailedWindowShow_RestoresItemAndRemovesSession()
    {
        var fixture = new Fixture();
        fixture.Window.ShowException = new InvalidOperationException("show failed");

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            fixture.Service.FloatAsync("main", fixture.Context.NavigationId));

        Assert.Equal(1, fixture.Region.AttachCount);
        Assert.Null(fixture.Window.Content);
        Assert.Empty(fixture.Service.FloatingViews);
        Assert.True(fixture.Window.WasDisposed);
    }

    private sealed class Fixture
    {
        public Fixture()
        {
            RenderedHost = new object();
            Context = new NavigationContext { RegionName = "main", ViewName = "editor" };
            var indicator = new Mock<IInnerRegionIndicatorHost>();
            indicator.SetupGet(x => x.Host).Returns(RenderedHost);
            Context.IndicatorHost.Value = indicator.Object;
            Item = new RegionPlacementItem(Context, 2, true);

            Region = new FakeRegion(Item);

            IRegion? regionValue = Region;
            var manager = new Mock<IRegionManager>();
            manager.Setup(x => x.TryGetRegion("main", out regionValue)).Returns(true);

            Window = new FakeWindowHost();
            var factory = new Mock<IFloatingWindowHostFactory>();
            factory.Setup(x => x.Create(It.IsAny<FloatingWindowOptions>())).Returns(Window);

            var provider = new ServiceCollection()
                .AddSingleton(manager.Object)
                .AddSingleton(factory.Object)
                .AddFloatingSupportCore()
                .BuildServiceProvider();
            Service = provider.GetRequiredService<IViewPlacementService>();
        }

        public object RenderedHost { get; }
        public NavigationContext Context { get; }
        public RegionPlacementItem Item { get; }
        public FakeRegion Region { get; }
        public FakeWindowHost Window { get; }
        public IViewPlacementService Service { get; }
    }

    private sealed class FakeRegion : IRegion, IRegionPlacementParticipant
    {
        private readonly RegionPlacementItem _item;

        public FakeRegion(RegionPlacementItem item) => _item = item;

        public int CaptureCount { get; private set; }
        public int DetachCount { get; private set; }
        public int AttachCount { get; private set; }
        public string Name => "main";
        public event EventHandler<NavigationEventArgs>? Navigated;
        IRegionPresenter IRegion.RegionPresenter => throw new NotSupportedException();

        public RegionPlacementItem Capture(Guid? navigationId = null)
        {
            CaptureCount++;
            return _item;
        }

        public void Detach(RegionPlacementItem item) => DetachCount++;
        public void Attach(RegionPlacementItem item, bool activate = true) => AttachCount++;
        public Task NavigateFromAsync(NavigationContext navigationContext) => Task.CompletedTask;
        public Task<NavigationResult> ActivateViewAsync(NavigationContext navigationContext) => throw new NotSupportedException();
        public Task<bool> CanGoBackAsync() => Task.FromResult(false);
        public Task<NavigationResult> GoBackAsync(CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task<bool> CanGoForwardAsync() => Task.FromResult(false);
        public Task<NavigationResult> GoForwardAsync(CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task RevertAsync(NavigationContext? navigationContext) => Task.CompletedTask;
        public void Dispose() { }
    }

    private sealed class FakeWindowHost : IFloatingWindowHost
    {
        public event EventHandler? RestoreRequested;
        public object? Content { get; private set; }
        public bool WasShown { get; private set; }
        public bool WasClosed { get; private set; }
        public bool WasDisposed { get; private set; }
        public int ActivateCount { get; private set; }
        public Exception? ShowException { get; set; }

        public Task SetContentAsync(object? content, CancellationToken cancellationToken = default)
        {
            Content = content;
            return Task.CompletedTask;
        }

        public Task ShowAsync(CancellationToken cancellationToken = default)
        {
            if (ShowException is not null) throw ShowException;
            WasShown = true;
            return Task.CompletedTask;
        }

        public Task ActivateAsync(CancellationToken cancellationToken = default)
        {
            ActivateCount++;
            return Task.CompletedTask;
        }

        public Task CloseAsync(CancellationToken cancellationToken = default)
        {
            WasClosed = true;
            return Task.CompletedTask;
        }

        public ValueTask DisposeAsync()
        {
            WasDisposed = true;
            return ValueTask.CompletedTask;
        }

        public void RequestRestore() => RestoreRequested?.Invoke(this, EventArgs.Empty);
    }
}
