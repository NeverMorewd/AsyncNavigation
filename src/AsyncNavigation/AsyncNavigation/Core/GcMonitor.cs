using System.Diagnostics;

namespace AsyncNavigation.Core;

public static class GcMonitor
{
    public static void Attach(object target, string? name = null)
    {
        if (target == null) return;

        if(string.IsNullOrEmpty(name))
            name = target.GetType().Name;

        _ = new Tracker(target, name);
    }

    private sealed class Tracker(object target, string name)
    {
        private readonly string _name = name;
        private readonly WeakReference _weakRef = new(target);

        ~Tracker()
        {
            if (_weakRef.IsAlive)
            {
                Debug.Fail($"[GcMonitor] {_name} has not been collected. Possible memory leak detected.");
            }
            else
            {
                Debug.WriteLine($"[GcMonitor] {_name} has been collected.");
            }
        }
    }
}
