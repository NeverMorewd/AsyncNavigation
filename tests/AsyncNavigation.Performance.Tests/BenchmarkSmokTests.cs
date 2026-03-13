using AsyncNavigation.Performance.Tests.Benchmarks;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Running;
using BenchmarkDotNet.Toolchains.InProcess.Emit;

namespace AsyncNavigation.Performance.Tests;

/// <summary>
/// Smoke-tests: runs each benchmark class in-process with a single, minimal
/// iteration so CI can verify the benchmarks compile and execute without errors.
/// These are NOT meant for measuring performance – use Release mode + BDN runner
/// for real measurements.
/// </summary>
public class BenchmarkSmokeTests
{
    private static IConfig SmokeConfig() =>
        ManualConfig.Create(DefaultConfig.Instance)
            .AddJob(Job.Dry
                .WithToolchain(InProcessEmitToolchain.Instance)
                .WithWarmupCount(0)
                .WithIterationCount(1)
                .WithInvocationCount(1)
                .WithUnrollFactor(1))
            .WithOption(ConfigOptions.DisableOptimizationsValidator, true);

    [Fact]
    public void RouterBenchmarks_Smoke()
    {
        var summary = BenchmarkRunner.Run<RouterBenchmarks>(SmokeConfig());
        Assert.True(summary.Reports.Length > 0, "Expected at least one benchmark report.");
        Assert.All(summary.Reports, r => Assert.True(r.Success, $"Benchmark '{r.BenchmarkCase.DisplayInfo}' failed."));
    }

    [Fact]
    public void ViewManagerBenchmarks_Smoke()
    {
        var summary = BenchmarkRunner.Run<ViewManagerBenchmarks>(SmokeConfig());
        Assert.True(summary.Reports.Length > 0);
        Assert.All(summary.Reports, r => Assert.True(r.Success, $"Benchmark '{r.BenchmarkCase.DisplayInfo}' failed."));
    }

    [Fact]
    public void RegionNavigationHistoryBenchmarks_Smoke()
    {
        var summary = BenchmarkRunner.Run<RegionNavigationHistoryBenchmarks>(SmokeConfig());
        Assert.True(summary.Reports.Length > 0);
        Assert.All(summary.Reports, r => Assert.True(r.Success, $"Benchmark '{r.BenchmarkCase.DisplayInfo}' failed."));
    }

    [Fact]
    public void NavigationContextBenchmarks_Smoke()
    {
        var summary = BenchmarkRunner.Run<NavigationContextBenchmarks>(SmokeConfig());
        Assert.True(summary.Reports.Length > 0);
        Assert.All(summary.Reports, r => Assert.True(r.Success, $"Benchmark '{r.BenchmarkCase.DisplayInfo}' failed."));
    }

    [Fact]
    public void AsyncJobProcessorBenchmarks_Smoke()
    {
        var summary = BenchmarkRunner.Run<AsyncJobProcessorBenchmarks>(SmokeConfig());
        Assert.True(summary.Reports.Length > 0);
        Assert.All(summary.Reports, r => Assert.True(r.Success, $"Benchmark '{r.BenchmarkCase.DisplayInfo}' failed."));
    }
}
