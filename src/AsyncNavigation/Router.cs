using AsyncNavigation.Abstractions;
using AsyncNavigation.Core;
using System.Collections.ObjectModel;

namespace AsyncNavigation;

public class Router : IRoute
{
    private readonly Dictionary<string, Route> _routes = [];

    public IRouteBuilder MapNavigation(string pathTemplate)
    {
        if (string.IsNullOrWhiteSpace(pathTemplate))
            throw new ArgumentException("Path template cannot be null or empty.", nameof(pathTemplate));

        return new RouteBuilder(this, pathTemplate);
    }

    public IReadOnlyList<Route> Routes => new ReadOnlyCollection<Route>([.. _routes.Values]);

    internal void Add(Route route)
    {
        ArgumentNullException.ThrowIfNull(route);
        route.Segments = string.IsNullOrEmpty(route.Path)
            ? []
            : route.Path.Split('/', StringSplitOptions.RemoveEmptyEntries);

        if (_routes.ContainsKey(route.Path))
        {
            throw new InvalidOperationException($"A route with path '{route.Path}' is already registered.");
        }

        _routes.Add(route.Path, route);
    }

    public Route? Match(string requestedPath)
    {
        if (string.IsNullOrEmpty(requestedPath))
        {
            requestedPath = string.Empty;
        }
        else
        {
            requestedPath = requestedPath.TrimStart('/');
            requestedPath = requestedPath.Length == 0 ? string.Empty : $"/{requestedPath}";
        }

        if (_routes.TryGetValue(requestedPath, out var exactMatch))
        {
            return exactMatch;
        }

        var cleanPath = requestedPath.Trim('/');
        var requestSegments = string.IsNullOrEmpty(cleanPath)
            ? []
            : cleanPath.Split('/', StringSplitOptions.RemoveEmptyEntries);

        foreach (var kvp in _routes.OrderByDescending(kvp => kvp.Value.Segments.Length))
        {
            if (TryMatchRoute(kvp.Value, requestSegments))
                return kvp.Value;
        }

        return null;
    }

    private static bool TryMatchRoute(Route route, string[] requestSegments)
    {
        if (route.Segments.Length != requestSegments.Length)
            return false;

        for (int i = 0; i < route.Segments.Length; i++)
        {
            var routeSeg = route.Segments[i];
            var reqSeg = requestSegments[i];

            if (!string.Equals(routeSeg, reqSeg, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }
        }

        return true;
    }
}