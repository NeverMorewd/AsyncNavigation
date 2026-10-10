using AsyncNavigation;
using AsyncNavigation.Abstractions;
using System.Diagnostics;

namespace Sample.Common;

public sealed class SampleNavigationInterceptor : INavigationInterceptor
{
    public Task OnNavigatingAsync(NavigationContext context)
    {
        Debug.WriteLine($"Interceptor: navigating to '{context.ViewName}' in '{context.RegionName}'");
        return Task.CompletedTask;
    }

    public Task OnNavigatedAsync(NavigationContext context)
    {
        Debug.WriteLine($"Interceptor: navigated to '{context.ViewName}' in '{context.RegionName}'");
        return Task.CompletedTask;
    }
}
