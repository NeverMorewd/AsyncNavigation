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
            var parameters = string.Equals(parts[1], "New", StringComparison.OrdinalIgnoreCase) ? new NavigationParameters { { "requestNew", true } } : null;
            return (parts[0], parameters);
        }
        return (param, null);
    }

    public static Task<NavigationResult> RequestNavigationCommandExecute(this IRegionManager regionManager, string regionName, string param, CancellationToken cancellationToken = default)
    {
        var (viewName, parameters) = ParseNavigationParam(param);
        return regionManager.RequestNavigateAsync(viewName, regionName, parameters, cancellationToken);
    }
}
