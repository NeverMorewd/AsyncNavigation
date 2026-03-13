using AsyncNavigation;
using AsyncNavigation.Core;
using AsyncNavigation.Performance.Tests;
using BenchmarkDotNet.Attributes;

namespace AsyncNavigation.Performance.Tests.Benchmarks;

/// <summary>
/// Measures RegionNavigationHistory throughput:
/// - Add() under normal conditions and when trimming is required
/// - GoBack / GoForward round-trip
/// - Clear()
/// </summary>
[Config(typeof(BenchmarkConfig))]
[HideColumns("Error", "StdDev", "Median", "RatioSD")]
public class RegionNavigationHistoryBenchmarks
{
    private RegionNavigationHistory _historySmall = null!;
    private RegionNavigationHistory _historyLarge = null!;
    private NavigationContext[] _contexts100 = null!;

    [GlobalSetup]
    public void Setup()
    {
        _historySmall = new RegionNavigationHistory(new NavigationOptions { MaxHistoryItems = 10 });
        _historyLarge = new RegionNavigationHistory(new NavigationOptions { MaxHistoryItems = 10_000 });

        _contexts100 = Enumerable.Range(0, 100)
            .Select(i => new NavigationContext { RegionName = "R", ViewName = $"V{i}" })
            .ToArray();

        foreach (var ctx in Enumerable.Range(0, 500)
                    .Select(i => new NavigationContext { RegionName = "R", ViewName = $"Pre{i}" }))
            _historyLarge.Add(ctx);
    }

    [Benchmark(Baseline = true)]
    public void Add_BelowCap()
    {
        _historySmall.Clear();
        for (int i = 0; i < 5; i++)
            _historySmall.Add(_contexts100[i]);
    }

    [Benchmark]
    public void Add_WithTrim()
    {
        _historySmall.Clear();
        for (int i = 0; i < _contexts100.Length; i++)
            _historySmall.Add(_contexts100[i]);
    }

    [Benchmark]
    public void GoBack_GoForward_RoundTrip()
    {
        for (int i = 0; i < 100; i++) _historyLarge.GoBack();
        for (int i = 0; i < 100; i++) _historyLarge.GoForward();
    }

    [Benchmark]
    public void Clear_LargeHistory()
    {
        var h = new RegionNavigationHistory(new NavigationOptions { MaxHistoryItems = 10_000 });
        foreach (var ctx in _contexts100)
            h.Add(ctx);
        h.Clear();
    }
}
