using System.Diagnostics;

namespace AsyncNavigation.Tests.Utils;

public static class GcUtils
{
    public static async Task AssertCollectedAsync(object obj)
    {
        var weak = new WeakReference(obj);
        Assert.True(await WaitForCollectedAsync(weak));
    }

    public static Task<bool> StartCollectAsync(object tartget, 
        int timeoutMs = 1000, 
        string? name = null)
    {
        var tracker = Attach(tartget, name);
        return WaitForCollectedAsync(tracker.WeakReference, timeoutMs);
    }
    public static Tracker Attach(object target, string? name = null)
    {
        ArgumentNullException.ThrowIfNull(target, nameof(target));

        if (string.IsNullOrEmpty(name))
            name = target.GetType().Name;

        return new Tracker(target, name);
        
    }

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

    public sealed class Tracker(object target, string name)
    {
        private readonly string _name = name;
        private readonly WeakReference _weakRef = new(target);

        ~Tracker()
        {
            if (_weakRef.IsAlive)
            {
                Debug.Fail($"[GcUtils] {_name} has not been collected. Possible memory leak detected.");
            }
            else
            {
                Debug.WriteLine($"[GcUtils] {_name} has been collected.");
            }
        }
        public WeakReference WeakReference => _weakRef;
    }
}
