using AsyncNavigation.Performance.Tests;
using AsyncNavigation.Performance.Tests.Benchmarks;
using BenchmarkDotNet.Running;

// -----------------------------------------------------------------------
// Entry point for standalone benchmark runs.
//
// Quick run (warmup=3, iterations=10) – default:
//   dotnet run -c Release --project tests/AsyncNavigation.Performance.Tests
//
// Full run (BDN defaults, higher accuracy):
//   dotnet run -c Release --project tests/AsyncNavigation.Performance.Tests -- --full
//
// Filter to a specific class or method:
//   dotnet run -c Release --project tests/AsyncNavigation.Performance.Tests -- --filter *Router*
//   dotnet run -c Release --project tests/AsyncNavigation.Performance.Tests -- --filter *ExactMatch*
//
// List all available benchmarks without running them:
//   dotnet run -c Release --project tests/AsyncNavigation.Performance.Tests -- --list flat
// -----------------------------------------------------------------------

// Allow --full to opt into unmodified BDN defaults
if (args.Contains("--full"))
{
    BenchmarkConfig.IsFull = true;
    args = args.Where(a => a != "--full").ToArray();
}

BenchmarkSwitcher
    .FromAssembly(typeof(RouterBenchmarks).Assembly)
    .Run(args, new BenchmarkConfig());
