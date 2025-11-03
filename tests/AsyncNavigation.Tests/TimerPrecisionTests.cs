using System.Diagnostics;
using Xunit.Abstractions;

namespace AsyncNavigation.Tests;
public class TimerPrecisionTests
{
    private readonly ITestOutputHelper _output;

    public TimerPrecisionTests(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public async Task MeasureTaskDelayPrecision()
    {
        const int iterations = 100;
        const int delayMs = 1;
        var deltas = new List<double>();

        _output.WriteLine($"Testing Task.Delay({delayMs}ms) precision, {iterations} iterations");
        _output.WriteLine($"Operating System: {Environment.OSVersion}");
        _output.WriteLine($".NET Version: {Environment.Version}");
        _output.WriteLine(new string('-', 60));

        var sw = Stopwatch.StartNew();
        for (int i = 0; i < iterations; i++)
        {
            var start = sw.Elapsed.TotalMilliseconds;
            await Task.Delay(delayMs);
            var end = sw.Elapsed.TotalMilliseconds;
            var actualDelay = end - start;
            deltas.Add(actualDelay);
        }

        PrintStatistics("Task.Delay", deltas, delayMs);
    }

    [Fact]
    public void MeasureThreadingTimerPrecision()
    {
        const int iterations = 100;
        const int intervalMs = 1;
        var deltas = new List<double>();
        var countdown = new CountdownEvent(iterations);
        var sw = Stopwatch.StartNew();
        double lastTick = 0;

        _output.WriteLine($"Testing System.Threading.Timer({intervalMs}ms) precision, {iterations} iterations");
        _output.WriteLine($"Operating System: {Environment.OSVersion}");
        _output.WriteLine($".NET Version: {Environment.Version}");
        _output.WriteLine(new string('-', 60));

        using var timer = new Timer(_ =>
        {
            var currentTick = sw.Elapsed.TotalMilliseconds;
            if (lastTick > 0)
            {
                deltas.Add(currentTick - lastTick);
            }
            lastTick = currentTick;
            countdown.Signal();
        }, null, 0, intervalMs);

        countdown.Wait(TimeSpan.FromSeconds(30));

        PrintStatistics("Threading.Timer", deltas, intervalMs);
    }

    [Fact]
    public async Task MeasurePeriodicTimerPrecisionAsync()
    {
        const int iterations = 100;
        const int intervalMs = 1;
        var deltas = new List<double>();

        _output.WriteLine($"Testing PeriodicTimer({intervalMs}ms) precision, {iterations} iterations");
        _output.WriteLine($"Operating System: {Environment.OSVersion}");
        _output.WriteLine($".NET Version: {Environment.Version}");
        _output.WriteLine(new string('-', 60));

        using var timer = new PeriodicTimer(TimeSpan.FromMilliseconds(intervalMs));
        var sw = Stopwatch.StartNew();
        double lastTick = 0;

        for (int i = 0; i < iterations; i++)
        {
            await timer.WaitForNextTickAsync().AsTask();
            var currentTick = sw.Elapsed.TotalMilliseconds;
            if (lastTick > 0)
            {
                deltas.Add(currentTick - lastTick);
            }
            lastTick = currentTick;
        }

        PrintStatistics("PeriodicTimer", deltas, intervalMs);
    }

    [Fact]
    public void MeasureSystemTimerResolution()
    {
        _output.WriteLine("Measure System Stopwatch Features:");
        _output.WriteLine($"Operating System: {Environment.OSVersion}");
        _output.WriteLine($".NET Version: {Environment.Version}");
        _output.WriteLine(new string('-', 60));

        _output.WriteLine($"Stopwatch Frequency: {Stopwatch.Frequency:N0} Hz");
        _output.WriteLine($"Stopwatch High Resolution: {Stopwatch.IsHighResolution}");
        _output.WriteLine($"Theoretical minimum resolution: {1000.0 / Stopwatch.Frequency:F6} ms");

        // Measure the actual smallest measurable time interval
        var sw = Stopwatch.StartNew();
        var minMeasurableDelta = double.MaxValue;

        for (int i = 0; i < 10000; i++)
        {
            var t1 = sw.Elapsed.TotalMilliseconds;
            var t2 = sw.Elapsed.TotalMilliseconds;
            var delta = t2 - t1;
            if (delta > 0 && delta < minMeasurableDelta)
            {
                minMeasurableDelta = delta;
            }
        }

        _output.WriteLine($"Actual smallest measurable interval: {minMeasurableDelta:F6} ms");
    }

    private void PrintStatistics(string timerType, List<double> deltas, int expectedMs)
    {
        if (deltas.Count == 0)
        {
            _output.WriteLine("No data collected");
            return;
        }

        var avg = deltas.Average();
        var min = deltas.Min();
        var max = deltas.Max();
        var stdDev = Math.Sqrt(deltas.Average(d => Math.Pow(d - avg, 2)));

        // Percentiles
        var sorted = deltas.OrderBy(x => x).ToList();
        var p50 = sorted[sorted.Count / 2];
        var p90 = sorted[(int)(sorted.Count * 0.9)];
        var p99 = sorted[(int)(sorted.Count * 0.99)];

        _output.WriteLine($"\n{timerType} Statistics (Expected: {expectedMs}ms):");
        _output.WriteLine($"  Sample Count: {deltas.Count}");
        _output.WriteLine($"  Average: {avg:F3} ms");
        _output.WriteLine($"  Minimum: {min:F3} ms");
        _output.WriteLine($"  Maximum: {max:F3} ms");
        _output.WriteLine($"  Std. Deviation: {stdDev:F3} ms");
        _output.WriteLine($"  Median (P50): {p50:F3} ms");
        _output.WriteLine($"  P90: {p90:F3} ms");
        _output.WriteLine($"  P99: {p99:F3} ms");
        _output.WriteLine($"  Error: {avg - expectedMs:F3} ms ({(avg - expectedMs) / expectedMs * 100:F1}%)");

        // Histogram
        _output.WriteLine("\nDistribution Histogram (ms):");
        var buckets = new Dictionary<int, int>();
        foreach (var delta in deltas)
        {
            var bucket = (int)Math.Floor(delta);
            buckets[bucket] = buckets.GetValueOrDefault(bucket, 0) + 1;
        }

        foreach (var kvp in buckets.OrderBy(x => x.Key))
        {
            var bar = new string('█', Math.Min(kvp.Value / 2, 50));
            _output.WriteLine($"  {kvp.Key,3}ms: {bar} ({kvp.Value})");
        }

        _output.WriteLine(new string('-', 60));
    }
}
