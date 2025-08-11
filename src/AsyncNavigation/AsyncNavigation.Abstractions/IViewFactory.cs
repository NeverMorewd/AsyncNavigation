namespace AsyncNavigation.Abstractions;

public interface IViewFactory
{
    Task<IView> CreateViewAsync(string viewName, CancellationToken cancellationToken = default);
    bool CanCreateView(string viewName);
}
