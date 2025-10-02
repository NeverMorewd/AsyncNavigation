namespace AsyncNavigation.Abstractions;

public interface IViewManager : IDisposable
{
    Task<IView> ResolveViewAsync(string key,
        bool useCache,
        Func<IView, Task<bool>>? isNavigationTarget = null,
        Func<IView, Task>? initialize = null);
    void AddView(string key, IView view);
    void Clear();
    void Remove(string key, bool dispose = false);
}
