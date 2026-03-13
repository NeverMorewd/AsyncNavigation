using AsyncNavigation.Performance.Tests.Benchmarks;
using BenchmarkDotNet.Running;

// Run all benchmarks when executed as a standalone app (Release mode).
// Usage:  dotnet run -c Release --project AsyncNavigation.Performance.Tests
BenchmarkSwitcher.FromAssembly(typeof(RouterBenchmarks).Assembly).RunAll();
