using AsyncNavigation.Abstractions;
using AsyncNavigation.Core;
using System.Diagnostics.CodeAnalysis;

namespace AsyncNavigation;

public static class NavigationContextExtensions
{
    public static bool TryResolveView(this NavigationContext navigationContext, [MaybeNullWhen(false)] out IView view)
    {
        if (navigationContext.Target.IsSet)
        {
            view = navigationContext.Target.Value!;
            return true;
        }
        view = default;
        return false;
    }

    public static bool TryResolveNavigationAware(this NavigationContext navigationContext, [MaybeNullWhen(false)] out INavigationAware aware)
    {
        if (navigationContext.Target.IsSet)
        {
            if(navigationContext.Target.Value is not null 
                && navigationContext.Target.Value.DataContext is INavigationAware navAware)
            {
                aware = navAware;
                return true;
            }            
        }
        aware = default;
        return false;
    }

    public static bool TryResolveViewAndAware(this NavigationContext navigationContext,
        [MaybeNullWhen(false)] out IView view,
        [MaybeNullWhen(false)] out INavigationAware aware)
    {
        view = default;
        aware = default;

        if (!navigationContext.Target.IsSet)
            return false;

        var target = navigationContext.Target.Value;
        if (target is null)
            return false;

        view = target;

        if (target.DataContext is INavigationAware navAware)
        {
            aware = navAware;
            return true;
        }
        return false;
    }
    public static NavigationContext WithParameter(this NavigationContext navigationContext, string key, object value)
    {
        if (navigationContext.IsCompleted && !navigationContext.IsForwordNavigation && !navigationContext.IsBackNavigation)
            throw new InvalidOperationException("Cannot add parameters after navigation is completed.");

        navigationContext.Parameters ??= new NavigationParameters();
        navigationContext.Parameters.Add(key, value);
        return navigationContext;
    }

    public static NavigationContext WithParameters(this NavigationContext navigationContext, IEnumerable<KeyValuePair<string, object>> parameters)
    {

        if (navigationContext.IsCompleted && !navigationContext.IsForwordNavigation && !navigationContext.IsBackNavigation)
            throw new InvalidOperationException("Cannot add parameters after navigation is completed.");

        navigationContext.Parameters ??= new NavigationParameters();
        navigationContext.Parameters.AddRange(parameters);
        return navigationContext;
    }

}
