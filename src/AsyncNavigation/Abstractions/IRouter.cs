using AsyncNavigation.Core;

namespace AsyncNavigation.Abstractions;
public interface IRouteMapper
{
    IRouteBuilder MapNavigation(string pathTemplate, params NavigationTarget[] targets);
}
public interface IRouteMatcher
{
    Route? Match(string requestedPath);
    IReadOnlyList<Route> Routes { get; }
}

public interface IRouter : IRouteMatcher, IRouteMapper
{

}
