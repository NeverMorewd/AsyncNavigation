using AsyncNavigation.Abstractions;
using AsyncNavigation.Core;

namespace Sample.Common;

public static class SampleHelper
{
    public static (string ViewName, INavigationParameters? Parameters) ParseNavigationParam(string param)
    {
        var parts = param.Split(':', 2, StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 0)
            return (param, null);

        var viewName = parts[0];
        NavigationParameters? parameters = null;

        if (parts.Length == 2)
        {
            var paramList = parts[1].Split(';', StringSplitOptions.RemoveEmptyEntries);
            foreach (var p in paramList)
            {
                if (string.Equals(p, "New", StringComparison.OrdinalIgnoreCase))
                {
                    parameters ??= [];
                    parameters.Add("requestNew", true);
                }
                else if (string.Equals(p, "Error", StringComparison.OrdinalIgnoreCase))
                {
                    parameters ??= [];
                    parameters.Add("raiseError", true);
                }
                else if (string.Equals(p, "Delay", StringComparison.OrdinalIgnoreCase))
                {
                    parameters ??= [];
                    parameters.Add("delay", TimeSpan.FromMilliseconds(2000));
                }
                else
                {
                    parameters ??= [];
                    parameters.Add(p, true);
                }
            }
        }

        return (viewName, parameters);
    }


    public static Task<NavigationResult> RequestNavigationCommandExecute(this IRegionManager regionManager, string regionName, string param, CancellationToken cancellationToken = default)
    {
        var (viewName, parameters) = ParseNavigationParam(param);
        return regionManager.RequestNavigateAsync(viewName, regionName, parameters, cancellationToken);
    }
}
