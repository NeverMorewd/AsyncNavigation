using AsyncNavigation;
using AsyncNavigation.Core;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;

namespace AsyncNavigation.Performance.Tests.Benchmarks;

/// <summary>
/// Measures RegionNavigationHistory throughput:
/// - Add() under normal conditions and when trimming is required (MaxHistoryItems exceeded)
/// - GoBack / GoForward round-trip
/// - Clear()
/// </summary>
[SimpleJob(RuntimeMoniker.Net80)]
[MemoryDiagnoser]
[HideColumns("Error", "StdDev", "Median", "RatioSD")]
public class RegionNavigationHistoryBenchmarks
{
    private RegionNavigationHistory _historySmall = null!;   // maxItems=10
    private RegionNavigationHistory _historyLarge = null!;   // maxItems=10_000

    // Pre-built contexts reused across iterations to isolate the history logic.
    private NavigationContext[] _contexts100 = null!;

    [GlobalSetup]
    public void Setup()
    {
        _historySmall = new RegionNavigationHistory(new NavigationOptions { MaxHistoryItems = 10 });
        _historyLarge = new RegionNavigationHistory(new NavigationOptions { MaxHistoryItems = 10_000 });

        _contexts100 = Enumerable.Range(0, 100)
            .Select(i => new NavigationContext { RegionName = "R", ViewName = $"V{i}" })
            .ToArray();

        // Pre-populate large history so GoBack/GoForward start from a realistic state
        foreach (var ctx in Enumerable.Range(0, 500)
                    .Select(i => new NavigationContext { RegionName = "R", ViewName = $"Pre{i}" }))
        {
            _historyLarge.Add(ctx);
        }
    }

    // -----------------------------------------------------------------------
    // Add – below cap (no trim)
    // -----------------------------------------------------------------------

    [Benchmark(Baseline = true)]
    public void Add_BelowCap()
    {
        _historySmall.Clear();
        for (int i = 0; i < 5; i++)
            _historySmall.Add(_contexts100[i]);
    }

    // -----------------------------------------------------------------------
    // Add – triggers MaxHistoryItems trim on every call
    // -----------------------------------------------------------------------

    [Benchmark]
    public void Add_WithTrim()
    {
        _historySmall.Clear();
        // Add more entries than maxItems so each add after the 10th trims
        for (int i = 0; i < _contexts100.Length; i++)
            _historySmall.Add(_contexts100[i]);
    }

    // -----------------------------------------------------------------------
    // GoBack / GoForward round-trip on large history
    // -----------------------------------------------------------------------

    [Benchmark]
    public void GoBack_GoForward_RoundTrip()
    {
        for (int i = 0; i < 100; i++) _historyLarge.GoBack();
        for (int i = 0; i < 100; i++) _historyLarge.GoForward();
    }

    // -----------------------------------------------------------------------
    // Clear on large history
    // -----------------------------------------------------------------------

    [Benchmark]
    public void Clear_LargeHistory()
    {
        // Re-populate each time so we measure clear cost consistently
        var h = new RegionNavigationHistory(new NavigationOptions { MaxHistoryItems = 10_000 });
        foreach (var ctx in _contexts100)
            h.Add(ctx);
        h.Clear();
    }
}
