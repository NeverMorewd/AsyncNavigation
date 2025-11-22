using AsyncNavigation.Core;

namespace AsyncNavigation.Abstractions;

public interface IRouteBuilder
{
    IRouteBuilder WithTargets(params NavigationTarget[] targets);
    IRouteBuilder WithFallback(NavigationTarget target);
}
