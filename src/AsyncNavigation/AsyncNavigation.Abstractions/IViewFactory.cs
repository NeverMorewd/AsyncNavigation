using System.Diagnostics.CodeAnalysis;

namespace AsyncNavigation.Abstractions;

public interface IViewFactory<T> where T : class
{
    bool TryUnWrapView(IView view, [MaybeNullWhen(false)] out T viewObject);
    T CreateViewObject(string viewName);
    IView CreateView(string viewName);
    bool CanCreateView(string viewName);
}
