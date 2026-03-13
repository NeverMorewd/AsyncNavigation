using AsyncNavigation;
using AsyncNavigation.Core;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;

namespace AsyncNavigation.Performance.Tests.Benchmarks;

/// <summary>
/// Measures Router.Match() throughput under various route-table sizes and
/// path types (exact hit, segment match, miss).
/// </summary>
[SimpleJob(RuntimeMoniker.Net80)]
[MemoryDiagnoser]
[HideColumns("Error", "StdDev", "Median", "RatioSD")]
public class RouterBenchmarks
{
    private Router _router10 = null!;
    private Router _router100 = null!;
    private Router _router1000 = null!;

    [GlobalSetup]
    public void Setup()
    {
        _router10   = BuildRouter(10);
        _router100  = BuildRouter(100);
        _router1000 = BuildRouter(1000);
    }

    private static Router BuildRouter(int count)
    {
        var r = new Router();
        for (int i = 0; i < count; i++)
            r.MapNavigation($"/segment{i}/page{i}");
        return r;
    }

    // -----------------------------------------------------------------------
    // Exact match (fast path via Dictionary)
    // -----------------------------------------------------------------------

    [Benchmark(Baseline = true)]
    [BenchmarkCategory("ExactMatch")]
    public object? ExactMatch_10Routes() => _router10.Match("/segment5/page5");

    [Benchmark]
    [BenchmarkCategory("ExactMatch")]
    public object? ExactMatch_100Routes() => _router100.Match("/segment50/page50");

    [Benchmark]
    [BenchmarkCategory("ExactMatch")]
    public object? ExactMatch_1000Routes() => _router1000.Match("/segment500/page500");

    // -----------------------------------------------------------------------
    // Miss (all routes checked)
    // -----------------------------------------------------------------------

    [Benchmark]
    [BenchmarkCategory("Miss")]
    public object? Miss_10Routes() => _router10.Match("/does/not/exist");

    [Benchmark]
    [BenchmarkCategory("Miss")]
    public object? Miss_100Routes() => _router100.Match("/does/not/exist");

    [Benchmark]
    [BenchmarkCategory("Miss")]
    public object? Miss_1000Routes() => _router1000.Match("/does/not/exist");

    // -----------------------------------------------------------------------
    // Registration cost
    // -----------------------------------------------------------------------

    [Benchmark]
    [BenchmarkCategory("Register")]
    public Router Register_100Routes() => BuildRouter(100);
}
