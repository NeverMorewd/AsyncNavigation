using AsyncNavigation;
using AsyncNavigation.Abstractions;
using AsyncNavigation.Core;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;

namespace AsyncNavigation.Performance.Tests.Benchmarks;

/// <summary>
/// Measures AsyncJobProcessor throughput for Queue and CancelCurrent strategies.
/// </summary>
[SimpleJob(RuntimeMoniker.Net80)]
[MemoryDiagnoser]
[HideColumns("Error", "StdDev", "Median", "RatioSD")]
public class AsyncJobProcessorBenchmarks
{
    [Params(1, 10, 100)]
    public int JobCount { get; set; }

    // -----------------------------------------------------------------------
    // Queue strategy – jobs run serially
    // -----------------------------------------------------------------------

    [Benchmark(Baseline = true)]
    public async Task Queue_Strategy()
    {
        var processor = new AsyncJobProcessor();
        var contexts = CreateContexts(JobCount);

        foreach (var ctx in contexts)
            await processor.RunJobAsync(ctx, _ => Task.CompletedTask, NavigationJobStrategy.Queue);

        DisposeAll(contexts);
    }

    // -----------------------------------------------------------------------
    // CancelCurrent strategy – each new job cancels the previous
    // -----------------------------------------------------------------------

    [Benchmark]
    public async Task CancelCurrent_Strategy()
    {
        var processor = new AsyncJobProcessor();
        var contexts = CreateContexts(JobCount);
        var tasks = new List<Task>(JobCount);

        foreach (var ctx in contexts)
        {
            tasks.Add(SafeRun(processor.RunJobAsync(ctx,
                async c => await Task.Delay(1, c.CancellationToken),
                NavigationJobStrategy.CancelCurrent)));
        }

        await Task.WhenAll(tasks);
        DisposeAll(contexts);
    }

    // -----------------------------------------------------------------------
    // Concurrent submissions with Queue strategy
    // -----------------------------------------------------------------------

    [Benchmark]
    public async Task Queue_ConcurrentSubmit()
    {
        var processor = new AsyncJobProcessor();
        var contexts = CreateContexts(JobCount);

        var submits = contexts.Select(ctx =>
            Task.Run(() => processor.RunJobAsync(ctx, _ => Task.CompletedTask, NavigationJobStrategy.Queue)));

        await Task.WhenAll(submits);
        DisposeAll(contexts);
    }

    // -----------------------------------------------------------------------
    // Helpers
    // -----------------------------------------------------------------------

    private static PerfJobContext[] CreateContexts(int count)
        => Enumerable.Range(0, count).Select(_ => new PerfJobContext()).ToArray();

    private static void DisposeAll(PerfJobContext[] contexts)
    {
        foreach (var c in contexts) c.Dispose();
    }

    private static async Task SafeRun(Task t)
    {
        try { await t; }
        catch (OperationCanceledException) { }
    }
}

/// <summary>Minimal IJobContext implementation used only in performance benchmarks.</summary>
internal sealed class PerfJobContext : IJobContext, IDisposable
{
    public Guid JobId { get; } = Guid.NewGuid();
    public CancellationToken CancellationToken { get; private set; } = CancellationToken.None;

    private CancellationTokenSource? _cts;

    public void OnStarted() { }
    public void OnCompleted() { }

    public void LinkCancellationToken(CancellationToken otherToken)
    {
        var newCts = CancellationTokenSource.CreateLinkedTokenSource(CancellationToken, otherToken);
        Interlocked.Exchange(ref _cts, newCts);
        CancellationToken = newCts.Token;
        // Old CTS is intentionally not disposed here to mirror the correct chain-preservation
        // approach used in NavigationContextForJob.
    }

    public void Dispose()
    {
        _cts?.Dispose();
        _cts = null;
    }
}
