using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Exporters;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Loggers;

namespace AsyncNavigation.Performance.Tests;

/// <summary>
/// Shared benchmark configuration used by all benchmark classes.
///
/// Tuning knobs:
///   WarmupCount   = 3   – enough to reach steady-state, avoids long auto-tune
///   IterationCount = 10  – 10 measured iterations per workload (was BDN default ~15+)
///   LaunchCount   = 1   – single process launch (no cross-process variance needed here)
///
/// To run with the full BDN defaults instead (longer but more statistically robust),
/// pass --full on the command line:
///   dotnet run -c Release -- --full --filter *Router*
/// </summary>
public class BenchmarkConfig : ManualConfig
{
    public static bool IsFull { get; set; }   // set by Program.cs when --full is passed

    public BenchmarkConfig()
    {
        var job = IsFull
            ? Job.Default.WithRuntime(BenchmarkDotNet.Environments.CoreRuntime.Core80)
            : Job.Default
                .WithRuntime(BenchmarkDotNet.Environments.CoreRuntime.Core80)
                .WithWarmupCount(3)
                .WithIterationCount(10)
                .WithLaunchCount(1);

        AddJob(job);
        AddLogger(ConsoleLogger.Default);
        AddColumnProvider(DefaultConfig.Instance.GetColumnProviders().ToArray());
        AddExporter(MarkdownExporter.GitHub);
        AddDiagnoser(MemoryDiagnoser.Default);
        // Suppress the "running in non-optimized mode" warning when Debug build
        WithOption(ConfigOptions.DisableOptimizationsValidator, true);
    }
}
