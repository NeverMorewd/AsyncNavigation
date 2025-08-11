using System.Diagnostics.CodeAnalysis;

namespace AsyncNavigation.Abstractions;

public interface IViewCacheManager
{
    bool TryCachedView(string cacheKey, [MaybeNullWhen(false)] out IView view);
    Task<IView?> GetCachedViewAsync(string cacheKey);
    Task SetCachedViewAsync(string cacheKey, IView view);
    void ClearCache();
    void RemoveFromCache(string cacheKey);
}
