namespace AsyncNavigation.Tests.Uitls;

public static class GcTestUtils
{
    public static async Task<bool> WaitForCollectedAsync(WeakReference weak, int timeoutMs = 1000)
    {
        var start = Environment.TickCount;

        while (weak.IsAlive && Environment.TickCount - start < timeoutMs)
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();

            if (!weak.IsAlive)
                return true;

            await Task.Delay(50);
        }

        return !weak.IsAlive;
    }
    public static bool WaitForCollected(WeakReference weak, int timeoutMs = 1000)
        => WaitForCollectedAsync(weak, timeoutMs).GetAwaiter().GetResult();
}
