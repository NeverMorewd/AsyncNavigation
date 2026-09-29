using AsyncNavigation.Abstractions;
using AsyncNavigation.Core;
using AsyncNavigation.Floating;
using Microsoft.Extensions.DependencyInjection;
using AsyncNavigation.Wpf.Floating;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using System.Runtime.ExceptionServices;
using ControlType = System.Windows.Controls.Control;
using PlatformContentRegion = AsyncNavigation.Wpf.ContentRegion;
using PlatformTabRegion = AsyncNavigation.Wpf.TabRegion;
namespace AsyncNavigation.Wpf.E2E.Tests;

public sealed class FloatingNavigationTests
{
    [Theory]
    [InlineData(false, false)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(true, true)]
    public void Floating_navigation_and_docking_preserve_instance_ownership(bool createNew, bool tabs) =>
        RunOnSta(() => VerifyNavigationAndDock(createNew, tabs));
    private static async Task VerifyNavigationAndDock(bool createNew, bool tabs)
    {
        var services = new ServiceCollection().AddNavigationSupport().AddFloatingSupportCore();
        services.RegisterView<TestView, Model>("A");
        services.RegisterView<TestView, Model>("B");
        services.AddSingleton<IFloatingWindowHostFactory, WindowFactory>();
        using var provider = services.BuildServiceProvider();
        var manager = provider.GetRequiredService<IRegionManager>();
        var placement = provider.GetRequiredService<IViewPlacementService>();
        var control = tabs ? (ControlType)new TabControl() : new ContentControl();
        IRegion region = tabs
            ? new PlatformTabRegion("main", (TabControl)control, provider, true)
            : new PlatformContentRegion("main", (ContentControl)control, provider, true);
        manager.AddRegion("main", region);
        var window = new Window { Content = control, Width = 500, Height = 400 };
        window.Show();
        try
        {
            var first = await manager.RequestNavigateAsync("main", "A");
            Assert.True(first.IsSuccessful, first.Exception?.ToString());
            var a1 = (TestView)first.NavigationContext!.Target.Value!;
            var session = await placement.FloatAsync("main");
            var floatingWindow = Window.GetWindow(a1);
            Assert.NotNull(floatingWindow);
            Assert.NotSame(window, floatingWindow);
            var next = await manager.RequestNavigateAsync("main", "B");
            Assert.True(next.IsSuccessful, next.Exception?.ToString());
            var b = next.NavigationContext!.Target.Value;
            var selected = ((IRegionPlacementParticipant)region).Capture().Context;

            if (createNew)
            {
                ((Model)a1.DataContext!).Reuse = false;
                var newA = await manager.RequestNavigateAsync("main", "A");
                Assert.True(newA.IsSuccessful, newA.Exception?.ToString());
                Assert.NotSame(a1, newA.NavigationContext!.Target.Value);
            }
            else
            {
                var reused = await manager.RequestNavigateAsync("main", "A");
                Assert.True(reused.IsSuccessful, reused.Exception?.ToString());
                Assert.Same(a1, reused.NavigationContext!.Target.Value);
                Assert.Same(selected, ((IRegionPlacementParticipant)region).Capture().Context);
                Assert.Same(floatingWindow, Window.GetWindow(a1));
            }

            await session.RestoreAsync();
            Dispatcher.CurrentDispatcher.Invoke(static () => { }, DispatcherPriority.ApplicationIdle);
            Assert.Equal(ViewPlacementState.Restored, session.State);
            Assert.False(floatingWindow!.IsVisible);
            Assert.Same(a1, ((IRegionPlacementParticipant)region).Capture().Context.Target.Value);
            Assert.Same(window, Window.GetWindow(a1));
            Assert.Equal(1, ((Model)a1.DataContext!).Initializations);
            if (tabs)
                Assert.Equal(createNew ? 3 : 2, ((TabControl)control).Items.Count);
            else
            {
                var back = await manager.GoBackAsync("main");
                Assert.True(back.IsSuccessful, back.Exception?.ToString());
                Assert.NotSame(a1, ((IRegionPlacementParticipant)region).Capture().Context.Target.Value);
            }
        }
        finally
        {
            foreach (var session in placement.FloatingViews.ToArray()) await session.CloseAsync();
            window.Close();
        }
    }

    public sealed class TestView : UserControl, IView
    {
        public TestView() => Content = new TextBox { Text = "state survives docking" };
    }

    public sealed class Model : INavigationAware
    {
        public bool Reuse { get; set; } = true;
        public int Initializations { get; private set; }
        public event AsyncEventHandler<AsyncEventArgs>? AsyncRequestUnloadEvent;
        public Task InitializeAsync(NavigationContext context) { Initializations++; return Task.CompletedTask; }
        public Task OnNavigatedToAsync(NavigationContext context) => Task.CompletedTask;
        public Task OnNavigatedFromAsync(NavigationContext context) => Task.CompletedTask;
        public Task<bool> IsNavigationTargetAsync(NavigationContext context) => Task.FromResult(Reuse);
        public Task OnUnloadAsync(CancellationToken token) => Task.CompletedTask;
    }

    private sealed class WindowFactory : IFloatingWindowHostFactory
    {
        public IFloatingWindowHost Create(FloatingWindowOptions options) => new WpfFloatingWindowHost(options);
    }
    private static void RunOnSta(Func<Task> action)
    {
        Exception? failure = null;
        var thread = new Thread(() =>
        {
            SynchronizationContext.SetSynchronizationContext(new DispatcherSynchronizationContext());
            var frame = new DispatcherFrame();
            async void Run()
            {
                try { await action(); }
                catch (Exception ex) { failure = ex; }
                finally { frame.Continue = false; }
            }
            Dispatcher.CurrentDispatcher.BeginInvoke((Action)Run);
            Dispatcher.PushFrame(frame);
            Dispatcher.CurrentDispatcher.InvokeShutdown();
        });
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        thread.Join();
        if (failure is not null) ExceptionDispatchInfo.Capture(failure).Throw();
    }
}
