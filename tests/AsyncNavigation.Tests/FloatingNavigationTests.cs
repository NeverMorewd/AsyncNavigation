using AsyncNavigation.Abstractions;
using AsyncNavigation.Core;
using AsyncNavigation.Floating;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace AsyncNavigation.Tests;

public sealed class FloatingNavigationTests
{
    [Fact]
    public async Task NavigateToFloatingInstance_ActivatesWindowWithoutLeavingCurrentView()
    {
        using var f = new Fixture();
        var a = await f.Navigate("A");
        var session = await f.Placement.FloatAsync("main");
        var b = await f.Navigate("B");
        b.Model.AllowLeave = false;
        var selected = f.Region.Selected;
        var historyCount = f.History.History.Count;

        var reused = await f.Navigate("A");

        Assert.Same(a, reused);
        Assert.Same(selected, f.Region.Selected);
        Assert.Same(b, f.Indicator.Content);
        Assert.Same(a, f.Windows[0].Content);
        Assert.Equal(1, f.Windows[0].Activations);
        Assert.Equal(0, b.Model.FromCount);
        Assert.Equal(historyCount, f.History.History.Count);
        Assert.Equal(ViewPlacementState.Floating, session.State);
    }

    [Theory]
    [InlineData("B")]
    [InlineData("A")]
    public async Task DockIntoOccupiedRegion_ReusesOriginalAndUpdatesCurrentAndHistory(string next)
    {
        using var f = new Fixture();
        var a = await f.Navigate("A");
        var session = await f.Placement.FloatAsync("main");
        a.Model.Reuse = false;
        var other = await f.Navigate(next);

        await session.RestoreAsync();

        Assert.NotSame(a, other);
        Assert.Same(a, f.Indicator.Content);
        Assert.Same(a, f.Region.Selected!.Target.Value);
        Assert.Same(f.Region.Selected, f.History.Current);
        Assert.Equal(1, other.Model.FromCount);
        Assert.Equal(1, a.Model.InitializeCount);
        Assert.Equal(2, a.Model.ToCount);
        Assert.True(f.Windows[0].Closed);
        Assert.Empty(f.Placement.FloatingViews);
        await ((IRegion)f.Region).GoBackAsync();
        Assert.Same(other, f.Indicator.Content);
        Assert.Equal(1, a.Model.FromCount);
    }

    [Fact]
    public async Task RestoredView_KeepsOriginalNavigationIdAndCanBeFloatedAgain()
    {
        using var f = new Fixture();
        var a = await f.Navigate("A");
        var navigationId = f.Region.Selected!.NavigationId;

        var session = await f.Placement.FloatAsync("main", navigationId);
        await session.RestoreAsync();

        Assert.Equal(navigationId, f.Region.Selected!.NavigationId);

        var secondSession = await f.Placement.FloatAsync("main", navigationId);

        Assert.Equal(ViewPlacementState.Floating, secondSession.State);
        Assert.Same(a, f.Windows[1].Content);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task FailedDock_PreservesBothViewsAndCanBeRetried(bool failAttach)
    {
        using var f = new Fixture();
        var a = await f.Navigate("A");
        var session = await f.Placement.FloatAsync("main");
        var b = await f.Navigate("B");
        var before = f.History.Current;
        b.Model.AllowLeave = failAttach;
        f.Region.FailNextAttach = failAttach;

        await Assert.ThrowsAnyAsync<Exception>(() => session.RestoreAsync());

        Assert.Same(b, f.Indicator.Content);
        Assert.Same(b, f.Region.Selected!.Target.Value);
        Assert.Same(a, f.Windows[0].Content);
        Assert.Same(before, f.History.Current);
        Assert.False(f.Windows[0].Closed);
        Assert.Equal(ViewPlacementState.Floating, session.State);
        b.Model.AllowLeave = true;
        await session.RestoreAsync();
        Assert.Same(a, f.Indicator.Content);
    }

    [Fact]
    public async Task CloseOldInstance_DoesNotRemoveNewInstanceCacheOrHistory()
    {
        using var f = new Fixture();
        var a1 = await f.Navigate("A");
        var session = await f.Placement.FloatAsync("main");
        a1.Model.Reuse = false;
        var a2 = await f.Navigate("A");

        await session.CloseAsync();
        var reused = await f.Navigate("A");

        Assert.True(a1.Disposed);
        Assert.False(a2.Disposed);
        Assert.Same(a2, reused);
        Assert.Same(a2, f.Indicator.Content);
        Assert.DoesNotContain(f.History.History, c => ReferenceEquals(c.Target.Value, a1));
    }

    [Fact]
    public async Task BackToFloatingView_ActivatesWindowAndKeepsHistoryPosition()
    {
        using var f = new Fixture();
        await f.Navigate("A");
        await f.Placement.FloatAsync("main");
        var b = await f.Navigate("B");
        var current = f.History.Current;

        await ((IRegion)f.Region).GoBackAsync();

        Assert.Same(b, f.Indicator.Content);
        Assert.Same(current, f.History.Current);
        Assert.Equal(1, f.Windows[0].Activations);
    }

    [Fact]
    public async Task CacheClear_DoesNotDisposeFloatingView()
    {
        using var f = new Fixture();
        var a = await f.Navigate("A");
        await f.Placement.FloatAsync("main");
        f.Cache.Clear();
        Assert.False(a.Disposed);
        Assert.Same(a, await f.Navigate("A"));
    }

    [Fact]
    public async Task NavigationWaitsForDockTransaction()
    {
        using var f = new Fixture();
        var a = await f.Navigate("A");
        var session = await f.Placement.FloatAsync("main");
        var b = await f.Navigate("B");
        var guard = new TaskCompletionSource<bool>();
        b.Model.GuardTask = guard.Task;
        var dock = session.RestoreAsync();
        var navigate = f.Navigate("C");
        Assert.False(dock.IsCompleted);
        Assert.False(navigate.IsCompleted);
        guard.SetResult(true);
        await dock;
        var c = await navigate;
        Assert.Same(c, f.Indicator.Content);
        Assert.Equal(1, a.Model.FromCount);
    }

    [Fact]
    public async Task NavigationCanStillCancelCurrentWhileFloatingEnabled()
    {
        using var f = new Fixture();
        var a = await f.Navigate("A");
        a.Model.GuardTask = new TaskCompletionSource<bool>().Task;
        var pending = f.Navigate("B");
        Assert.False(pending.IsCompleted);
        a.Model.GuardTask = null;
        var replacement = f.Navigate("C");
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => pending);
        var c = await replacement;
        Assert.Same(c, f.Indicator.Content);
    }

    [Fact]
    public async Task CacheEviction_PinsFloatingViewAndDoesNotDisposeNewView()
    {
        using var f = new Fixture(maxCacheSize: 1);
        var a = await f.Navigate("A");
        await f.Placement.FloatAsync("main");
        var b = await f.Navigate("B");
        Assert.False(a.Disposed);
        Assert.False(b.Disposed);
        Assert.Same(a, await f.Navigate("A"));
    }

    [Fact]
    public async Task CancelledDock_PreservesRegionAndFloatingWindow()
    {
        using var f = new Fixture();
        var a = await f.Navigate("A");
        var session = await f.Placement.FloatAsync("main");
        var b = await f.Navigate("B");
        b.Model.GuardTask = new TaskCompletionSource<bool>().Task;
        using var cancellation = new CancellationTokenSource();
        var dock = session.RestoreAsync(cancellation.Token);
        cancellation.Cancel();
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => dock);
        Assert.Same(b, f.Indicator.Content);
        Assert.Same(a, f.Windows[0].Content);
        Assert.Equal(ViewPlacementState.Floating, session.State);
    }

    private sealed class Fixture : IDisposable
    {
        private readonly ServiceProvider _provider;
        public Fixture(int maxCacheSize = 20)
        {
            var manager = new Mock<IRegionManager>();
            var windows = new Mock<IFloatingWindowHostFactory>();
            windows.Setup(x => x.Create(It.IsAny<FloatingWindowOptions>())).Returns(() =>
            {
                var window = new WindowHost();
                Windows.Add(window);
                return window;
            });
            var services = new ServiceCollection()
                .AddSingleton(new NavigationOptions { ViewCacheStrategy = ViewCacheStrategy.UpdateDuplicateKey, MaxCachedViews = maxCacheSize })
                .AddSingleton<IViewFactory, ViewFactory>()
                .AddSingleton<IRegionManager>(manager.Object)
                .AddSingleton<IFloatingWindowHostFactory>(windows.Object)
                .AddSingleton<IViewManager, ViewManager>()
                .AddSingleton<IRegionNavigationHistory, RegionNavigationHistory>()
                .AddSingleton<IRegionNavigationServiceFactory, RegionNavigationServiceFactory>()
                .AddTransient<IAsyncJobProcessor, AsyncJobProcessor>()
                .AddTransient<IRegionIndicatorManager, RegionIndicatorManager>()
                .AddSingleton<IInnerRegionIndicatorHost>(Indicator)
                .AddFloatingSupportCore();
            _provider = services.BuildServiceProvider();
            Region = new ContentRegion(_provider);
            IRegion? region = Region;
            manager.Setup(x => x.TryGetRegion("main", out region)).Returns(true);
            Placement = _provider.GetRequiredService<IViewPlacementService>();
            History = _provider.GetRequiredService<IRegionNavigationHistory>();
            Cache = _provider.GetRequiredService<IViewManager>();
        }
        public ContentRegion Region { get; }
        public IndicatorHost Indicator { get; } = new();
        public List<WindowHost> Windows { get; } = [];
        public IViewPlacementService Placement { get; }
        public IRegionNavigationHistory History { get; }
        public IViewManager Cache { get; }
        public async Task<View> Navigate(string name)
        {
            var context = new NavigationContext { RegionName = "main", ViewName = name };
            await ((IRegion)Region).ActivateViewAsync(context);
            return (View)context.Target.Value!;
        }
        public void Dispose() => _provider.Dispose();
    }

    private sealed class ContentRegion(IServiceProvider provider)
        : RegionBase<ContentRegion, object>("main", new object(), provider), IRegionPlacementParticipant
    {
        public NavigationContext? Selected { get; private set; }
        public bool FailNextAttach { get; set; }
        public override NavigationPipelineMode NavigationPipelineMode => NavigationPipelineMode.RenderFirst;
        protected override void InitializeOnRegionCreated(object control)
        {
            EnableViewCache = true;
            IsSinglePageRegion = true;
        }
        public override Task ProcessActivateAsync(NavigationContext context)
        {
            Selected = context;
            return Task.CompletedTask;
        }
        public override Task ProcessDeactivateAsync(NavigationContext? context)
        {
            if (context is null || ReferenceEquals(Selected, context)) Selected = null;
            return Task.CompletedTask;
        }
        public RegionPlacementItem Capture(Guid? id = null)
        {
            var context = Selected ?? throw new InvalidOperationException("empty");
            if (id.HasValue && context.NavigationId != id.Value)
                throw new InvalidOperationException($"Region '{Name}' does not contain the requested navigation item.");
            return new(context, 0, true);
        }
        public void Detach(RegionPlacementItem item)
        {
            Assert.Same(Selected, item.Context);
            Selected = null;
        }
        public void Attach(RegionPlacementItem item, bool activate = true)
        {
            if (FailNextAttach)
            {
                FailNextAttach = false;
                throw new InvalidOperationException("attach failed");
            }
            Assert.Null(Selected);
            Selected = item.Context;
        }
    }

    private sealed class ViewFactory : IViewFactory
    {
        public IView CreateView(string key) => new View();
        public bool CanCreateView(string key) => true;
        public void AddView(string key, IView view) => throw new NotSupportedException();
        public void AddView(string key, Func<string, IView> factory) => throw new NotSupportedException();
    }

    private sealed class View : IView, IDisposable
    {
        public Model Model { get; } = new();
        public object? DataContext { get => Model; set => throw new NotSupportedException(); }
        public bool Disposed { get; private set; }
        public void Dispose() => Disposed = true;
    }
    private sealed class Model : INavigationAware, INavigationGuard
    {
        public bool AllowLeave { get; set; } = true;
        public bool Reuse { get; set; } = true;
        public int InitializeCount { get; private set; }
        public int ToCount { get; private set; }
        public int FromCount { get; private set; }
        public Task<bool>? GuardTask { get; set; }
        public event AsyncEventHandler<AsyncEventArgs>? AsyncRequestUnloadEvent;
        public Task InitializeAsync(NavigationContext context) { InitializeCount++; return Task.CompletedTask; }
        public Task OnNavigatedToAsync(NavigationContext context) { ToCount++; return Task.CompletedTask; }
        public Task OnNavigatedFromAsync(NavigationContext context) { FromCount++; return Task.CompletedTask; }
        public Task<bool> IsNavigationTargetAsync(NavigationContext context) => Task.FromResult(Reuse);
        public Task OnUnloadAsync(CancellationToken token) => Task.CompletedTask;
        public Task<bool> CanNavigateAsync(NavigationContext context, CancellationToken token) => GuardTask?.WaitAsync(token) ?? Task.FromResult(AllowLeave);
    }
    private sealed class IndicatorHost : IInnerRegionIndicatorHost, IRegionPlacementContentHost
    {
        public object Host => this;
        public object? Content { get; private set; }
        public object DetachContent() { var content = Content ?? throw new InvalidOperationException("empty"); Content = null; return content; }
        public void AttachContent(object content) { Assert.Null(Content); Content = content; }
        public Task ShowContentAsync(NavigationContext context) { Content = context.Target.Value; return Task.CompletedTask; }
        public Task ShowErrorAsync(NavigationContext context, Exception? exception) => Task.CompletedTask;
        public Task ShowLoadingAsync(NavigationContext context) => Task.CompletedTask;
        public Task OnLoadedAsync(NavigationContext context) => Task.CompletedTask;
        public Task OnCancelledAsync(NavigationContext context) => Task.CompletedTask;
    }
    private sealed class WindowHost : IFloatingWindowHost
    {
        public event EventHandler? RestoreRequested;
        public event EventHandler? CloseRequested;
        public object? Content { get; private set; }
        public bool Closed { get; private set; }
        public int Activations { get; private set; }
        public Task SetContentAsync(object? content, CancellationToken token = default) { Content = content; return Task.CompletedTask; }
        public Task ShowAsync(CancellationToken token = default) => Task.CompletedTask;
        public Task ActivateAsync(CancellationToken token = default) { Activations++; return Task.CompletedTask; }
        public Task CloseAsync(CancellationToken token = default) { Closed = true; return Task.CompletedTask; }
        public ValueTask DisposeAsync() => ValueTask.CompletedTask;
    }
}
