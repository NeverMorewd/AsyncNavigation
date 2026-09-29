namespace AsyncNavigation.Abstractions;

/// <summary>Optional instance-aware cache operations used by floating navigation.</summary>
public interface IViewPlacementCache
{
    /// <summary>Finds a reusable instance without creating or initializing a view.</summary>
    Task<IView?> FindCachedViewAsync(string key, Func<IView, Task<bool>> isNavigationTarget);

    /// <summary>Removes entries for exactly this instance without disposing another same-name view.</summary>
    void RemoveInstance(IView view);
}
