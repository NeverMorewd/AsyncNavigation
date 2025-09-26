using AsyncNavigation.Abstractions;
using AsyncNavigation.Core;

namespace Sample.Common;

public static class SampleHelper
{
    public static (string ViewName, INavigationParameters? Parameters) ParseNavigationParam(string param)
    {
        var parts = param.Split(':', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 2)
        {
            if (string.Equals(parts[1], "New", StringComparison.OrdinalIgnoreCase))
            {
                return (parts[0], new NavigationParameters { { "requestNew", true } });
            }
            if (string.Equals(parts[1], "Error", StringComparison.OrdinalIgnoreCase))
            {
                return (parts[0], new NavigationParameters { { "raiseError", true } });
            }
            return (parts[0], null);
        }
        return (param, null);
    }

    public static Task<NavigationResult> RequestNavigationCommandExecute(this IRegionManager regionManager, string regionName, string param, CancellationToken cancellationToken = default)
    {
        var (viewName, parameters) = ParseNavigationParam(param);
        return regionManager.RequestNavigateAsync(viewName, regionName, parameters, cancellationToken);
    }
}
