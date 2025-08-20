using AsyncNavigation.Core;
using System.Diagnostics.CodeAnalysis;

namespace AsyncNavigation.Abstractions;

public interface IViewManager : IDisposable
{
    Task<IView> ResolveViewAsync(string key, bool useCache, NavigationContext navigationContext);
    void AddView(string key, IView view);
    void Clear();
    void Remove(string key, bool dispose = false);
}
