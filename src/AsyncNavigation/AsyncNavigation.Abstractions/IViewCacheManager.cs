using System.Diagnostics.CodeAnalysis;

namespace AsyncNavigation.Abstractions;

public interface IViewCacheManager
{
    bool TryAddView(string key, [MaybeNullWhen(false)] out IView view);
    Task<IView?> GetView(string key);
    Task SetView(string key, IView view);
    void Clear();
    void Remove(string key);
}
