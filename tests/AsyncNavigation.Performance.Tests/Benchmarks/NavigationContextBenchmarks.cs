using AsyncNavigation;
using AsyncNavigation.Abstractions;
using AsyncNavigation.Core;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;

namespace AsyncNavigation.Performance.Tests.Benchmarks;

/// <summary>
/// Measures NavigationContext construction and cancellation-token chaining cost.
/// Specifically validates that multiple LinkCancellationToken() calls are cheap
/// and do not cause unexpected allocations (e.g. from over-eager disposal).
/// </summary>
[SimpleJob(RuntimeMoniker.Net80)]
[MemoryDiagnoser]
[HideColumns("Error", "StdDev", "Median", "RatioSD")]
public class NavigationContextBenchmarks
{
    private CancellationTokenSource _cts1 = null!;
    private CancellationTokenSource _cts2 = null!;
    private CancellationTokenSource _cts3 = null!;

    [GlobalSetup]
    public void Setup()
    {
        _cts1 = new CancellationTokenSource();
        _cts2 = new CancellationTokenSource();
        _cts3 = new CancellationTokenSource();
    }

    [GlobalCleanup]
    public void Cleanup()
    {
        _cts1.Dispose();
        _cts2.Dispose();
        _cts3.Dispose();
    }

    // -----------------------------------------------------------------------
    // Construction
    // -----------------------------------------------------------------------

    [Benchmark(Baseline = true)]
    public NavigationContext Construct()
        => new() { RegionName = "Main", ViewName = "View" };

    // -----------------------------------------------------------------------
    // Single link
    // -----------------------------------------------------------------------

    [Benchmark]
    public NavigationContext Link_Single()
    {
        var ctx = new NavigationContext { RegionName = "Main", ViewName = "View" };
        ctx.LinkCancellationToken(_cts1.Token);
        return ctx;
    }

    // -----------------------------------------------------------------------
    // Double link – the real-world flow (job token + request token)
    // -----------------------------------------------------------------------

    [Benchmark]
    public NavigationContext Link_Double()
    {
        var ctx = new NavigationContext { RegionName = "Main", ViewName = "View" };
        ctx.LinkCancellationToken(_cts1.Token);
        ctx.LinkCancellationToken(_cts2.Token);
        return ctx;
    }

    // -----------------------------------------------------------------------
    // Triple link
    // -----------------------------------------------------------------------

    [Benchmark]
    public NavigationContext Link_Triple()
    {
        var ctx = new NavigationContext { RegionName = "Main", ViewName = "View" };
        ctx.LinkCancellationToken(_cts1.Token);
        ctx.LinkCancellationToken(_cts2.Token);
        ctx.LinkCancellationToken(_cts3.Token);
        return ctx;
    }

    // -----------------------------------------------------------------------
    // Full lifecycle: link → start → complete (includes CTS disposal)
    // -----------------------------------------------------------------------

    [Benchmark]
    public void FullLifecycle()
    {
        var ctx = new NavigationContext { RegionName = "Main", ViewName = "View" };
        ctx.LinkCancellationToken(_cts1.Token);
        ctx.LinkCancellationToken(_cts2.Token);

        var job = (IJobContext)ctx;
        job.OnStarted();
        job.OnCompleted();
    }
}
