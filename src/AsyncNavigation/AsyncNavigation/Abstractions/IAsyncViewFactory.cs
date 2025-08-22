namespace AsyncNavigation.Abstractions;

public interface IAsyncViewFactory
{
    Task<IView> CreateViewAsync(string viewName, NavigationContext context, CancellationToken cancellationToken = default);
}
