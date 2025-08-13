using System.Diagnostics.CodeAnalysis;

namespace AsyncNavigation.Abstractions;

public interface IViewFactory
{
    IView CreateView(string viewName);
    bool CanCreateView(string viewName);
    void AddView(string key, IView view);
    void AddView(string key, Func<string, IView> viewBuilder);
}
