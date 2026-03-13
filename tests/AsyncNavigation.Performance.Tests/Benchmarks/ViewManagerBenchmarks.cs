using AsyncNavigation;
using AsyncNavigation.Abstractions;
using AsyncNavigation.Core;
using AsyncNavigation.Performance.Tests;
using BenchmarkDotNet.Attributes;
using Microsoft.Extensions.DependencyInjection;

namespace AsyncNavigation.Performance.Tests.Benchmarks;

/// <summary>
/// Measures ViewManager throughput:
/// - cache-hit resolve (Touch / LRU update path)
/// - cache-miss resolve (create + AddToLru path + possible eviction)
/// </summary>
[Config(typeof(BenchmarkConfig))]
[HideColumns("Error", "StdDev", "Median", "RatioSD")]
public class ViewManagerBenchmarks
{
    private IViewManager _manager = null!;

    private const int ViewCount = 20;
    private const int CacheSize = 10;

    [GlobalSetup]
    public void Setup()
    {
        var sc = new ServiceCollection();
        NavigationOptions.Default.MaxCachedViews = CacheSize;
        sc.AddSingleton(NavigationOptions.Default);
        sc.AddSingleton<IAsyncJobProcessor, AsyncJobProcessor>();
        sc.AddSingleton<IViewFactory, ViewFactory>();
        sc.AddSingleton<IRegistrationTracker>(RegistrationTracker.Instance);
        sc.AddTransient<IViewManager, ViewManager>();

        for (int i = 0; i < ViewCount; i++)
            RegisterBenchView(sc, $"View{i}");

        _manager = sc.BuildServiceProvider().GetRequiredService<IViewManager>();

        // Pre-warm the cache
        for (int i = 0; i < CacheSize; i++)
            _manager.ResolveViewAsync($"View{i}", useCache: true).GetAwaiter().GetResult();
    }

    [GlobalCleanup]
    public void Cleanup() => _manager.Dispose();

    [Benchmark(Baseline = true)]
    public async Task<IView> Resolve_CacheHit()
        => await _manager.ResolveViewAsync("View0", useCache: true);

    private int _missCounter;

    [Benchmark]
    public async Task<IView> Resolve_CacheMiss_WithEviction()
    {
        var key = $"View{(_missCounter++ % ViewCount)}";
        return await _manager.ResolveViewAsync(key, useCache: false);
    }

    [Benchmark]
    public void Remove_ExistingKey()
    {
        _manager.ResolveViewAsync("View0", useCache: true).GetAwaiter().GetResult();
        _manager.Remove("View0", dispose: false);
    }

    [Benchmark]
    public void AddView_UpdateDuplicate()
        => _manager.AddView("View0", new BenchView());

    private static void RegisterBenchView(IServiceCollection sc, string name)
    {
        sc.AddTransient<BenchViewModel>();
        sc.AddTransient<BenchView>();
        sc.AddKeyedTransient<IView>(name, (sp, _) =>
        {
            var v = sp.GetRequiredService<BenchView>();
            v.DataContext = sp.GetRequiredService<BenchViewModel>();
            return v;
        });
        RegistrationTracker.Instance.TrackView(name);
    }
}

internal sealed class BenchView : IView
{
    public object? DataContext { get; set; }
}

internal sealed class BenchViewModel : INavigationAware
{
    public event AsyncEventHandler<AsyncEventArgs>? AsyncRequestUnloadEvent;
    public Task InitializeAsync(NavigationContext context) => Task.CompletedTask;
    public Task<bool> IsNavigationTargetAsync(NavigationContext context) => Task.FromResult(true);
    public Task OnNavigatedFromAsync(NavigationContext context) => Task.CompletedTask;
    public Task OnNavigatedToAsync(NavigationContext context) => Task.CompletedTask;
    public Task OnUnloadAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
