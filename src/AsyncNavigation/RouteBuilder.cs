using AsyncNavigation.Abstractions;
using AsyncNavigation.Core;
using System.Text;

namespace AsyncNavigation;

public class RouteBuilder : IRouteBuilder
{
    private readonly Router _router;
    private readonly string _template;
    private NavigationTarget? _fallbackStep;

    internal RouteBuilder(Router router, string template)
    {
        _router = router;
        _template = NormalizePath(template);
    }


    public IRouteBuilder WithFallback(NavigationTarget step)
    {
        ArgumentNullException.ThrowIfNull(step);
        _fallbackStep = step;
        return this;
    }

    public IRouteBuilder WithTargets(params NavigationTarget[] steps)
    {
        if (steps == null || steps.Length == 0)
            throw new ArgumentException("At least one navigation step is required.", nameof(steps));

        var route = new Route
        {
            Path = _template,
            Targets = steps.AsReadOnly(),
            Fallback = _fallbackStep
        };

        _router.Add(route);
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