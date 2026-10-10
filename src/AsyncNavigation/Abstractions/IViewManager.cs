namespace AsyncNavigation.Abstractions;

public interface IViewManager : IDisposable
{
    Task<IView> ResolveViewAsync(string key,
        bool useCache,
        Func<IView, Task<bool>>? isNavigationTarget = null,
        Func<IView, Task>? initialize = null);
    void AddView(string key, IView view);
    void Clear();
    /// <summary>Removes the cache entry for <paramref name="key"/>. Returns false without removing
    /// anything if the cached instance is currently floating (pinned) or the key isn't cached.</summary>
    bool Remove(string key, bool dispose = false);
}
