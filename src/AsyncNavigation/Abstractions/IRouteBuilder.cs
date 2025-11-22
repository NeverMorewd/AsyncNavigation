using AsyncNavigation.Core;

namespace AsyncNavigation.Abstractions;

public interface IRouteBuilder
{
    IRouteBuilder WithSegments(params string[] segments);
    IRouteBuilder WithFallback(NavigationTarget target);
}
