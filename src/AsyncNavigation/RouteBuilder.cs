using AsyncNavigation.Abstractions;
using AsyncNavigation.Core;
using System.Data;
using System.Text;

namespace AsyncNavigation;

public class RouteBuilder : IRouteBuilder
{
    private readonly Router _router;

    private readonly Route _route;

    internal RouteBuilder(Router router, string template, params NavigationTarget[] targets)
    {
        if (targets == null || targets.Length == 0)
            throw new ArgumentException("At least one navigation target is required.", nameof(targets));
        _router = router;
        _route = new Route
        {
            Path = NormalizePath(template),
            Targets = targets.AsReadOnly()
        };
        _router.Add(_route);
    }

    public IRouteBuilder WithFallback(NavigationTarget target)
    {
        ArgumentNullException.ThrowIfNull(target);
        _route.Fallback = target;
        return this;
    }
    public IRouteBuilder WithSegments(params string[] segments)
    {
        if (segments == null || segments.Length == 0)
            throw new ArgumentException("At least one segment is required.", nameof(segments));

        _route.Segments = segments;
        return this;
    }
    private static string NormalizePath(string? path)
    {
        if (string.IsNullOrEmpty(path))
            return string.Empty;

        var sb = new StringBuilder();
        int start = 0;
        int end = path.Length;

        while (start < end && path[start] == '/')
            start++;
        while (end > start && path[end - 1] == '/')
            end--;

        if (start >= end)
            return string.Empty;

        sb.Append('/');
        bool lastWasSlash = false;
        for (int i = start; i < end; i++)
        {
            char c = path[i];
            if (c == '/')
            {
                if (!lastWasSlash)
                {
                    sb.Append(c);
                    lastWasSlash = true;
                }
            }
            else
            {
                sb.Append(c);
                lastWasSlash = false;
            }
        }

        return sb.ToString();
    }
}