using AsyncNavigation.Abstractions;
using AsyncNavigation.Core;
using System.Collections.Concurrent;

namespace AsyncNavigation;

/// <summary>
/// Thread-safe Singleton implementation for RegistrationTracker.
/// </summary>
public class RegistrationTracker : IRegistrationTracker
{
    private static readonly Lazy<RegistrationTracker> _instance = new(() => new RegistrationTracker());
    internal static RegistrationTracker Instance => _instance.Value;

    private readonly ConcurrentDictionary<RegistryCategory, ConcurrentBag<string>> _categoryRegistry = new();

    private RegistrationTracker() { }

    public void Register(RegistryCategory category, string key)
    {
        var bag = _categoryRegistry.GetOrAdd(category, _ => []);
        bag.Add(key);
    }

    public IReadOnlyDictionary<RegistryCategory, IReadOnlyCollection<string>> GetAll()
    {
        var dict = new Dictionary<RegistryCategory, IReadOnlyCollection<string>>();
        foreach (var kvp in _categoryRegistry)
        {
            dict.Add(kvp.Key, [.. kvp.Value]);
        }
        return dict;
    }
    internal void Clear()
    {
        _categoryRegistry.Clear();
    }
}