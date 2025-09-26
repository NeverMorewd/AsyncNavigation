using AsyncNavigation.Abstractions;
using AsyncNavigation.Core;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Concurrent;

namespace AsyncNavigation;

internal sealed class RegionIndicatorManager : IRegionIndicatorManager
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IInnerRegionIndicatorHost _innerRegionIndicatorHost;
    private readonly ConcurrentDictionary<NavigationContext, (IInnerRegionIndicatorHost Inner, IEnumerable<IRegionIndicator> Others)> _cachedIndicators;

    public RegionIndicatorManager(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        _cachedIndicators = new ConcurrentDictionary<NavigationContext, (IInnerRegionIndicatorHost Inner, IEnumerable<IRegionIndicator> Others)>();
        _innerRegionIndicatorHost = _serviceProvider.GetRequiredService<IInnerRegionIndicatorHost>();
    }

    public void Setup(NavigationContext context, bool useSingleton)
    {
        if (context.IndicatorHost.IsSet)
        {
            return;
        }
        if (useSingleton)
        {
            context.IndicatorHost.Value = _innerRegionIndicatorHost;
        }
        else
        {
            context.IndicatorHost.Value = _serviceProvider.GetRequiredService<IInnerRegionIndicatorHost>();
        }
    }

    public Task ShowErrorAsync(NavigationContext context, Exception exception)
    {
        var (Inner, Others) = _cachedIndicators.GetOrAdd(context, ResolveRegionIndicators(context));
        return ShowErrorCore(Inner, Others, context, exception);
    }

    public async Task StartAsync(NavigationContext context, Task processTask, TimeSpan? delayTime = null)
    {
        var (Inner, Others) = _cachedIndicators.GetOrAdd(context, ResolveRegionIndicators(context));
        if (delayTime.HasValue)
        {
            var delayTask = Task.Delay(delayTime.Value, context.CancellationToken);
            if (await Task.WhenAny(processTask, delayTask) == delayTask && !processTask.IsCompleted)
            {
                await ShowLoadingCore(Inner, Others, context);
            }
        }
        else
        {
            await ShowLoadingCore(Inner, Others, context);
        }
        await processTask;
        await OnLoadedCore(Inner, Others, context);
        await ShowContentCore(Inner, context);
    }
    private static async Task ShowErrorCore(IRegionIndicator inner, IEnumerable<IRegionIndicator> others, NavigationContext context, Exception exception)
    {
        await Task.WhenAll(others.Append(inner).Select(indicator => indicator.ShowErrorAsync(context, exception)));
    }
    private (IInnerRegionIndicatorHost Inner, IEnumerable<IRegionIndicator> Others) ResolveRegionIndicators(NavigationContext context)
    {
        var inner = ResolveInnerIndicator(context);
        var regionIndicators = ResolveIndicators(context);
        return (inner, regionIndicators);
    }
    private static async Task ShowLoadingCore(IRegionIndicator inner, IEnumerable<IRegionIndicator> others , NavigationContext context)
    {
        await Task.WhenAll(others.Append(inner).Select(indicator => indicator.ShowLoadingAsync(context)));
    }
    private static async Task OnLoadedCore(IRegionIndicator inner, IEnumerable<IRegionIndicator> others, NavigationContext context)
    {
        await Task.WhenAll(others.Append(inner).Select(indicator => indicator.OnLoadedAsync(context)));
    }
    private static async Task ShowContentCore(IInnerRegionIndicatorHost inner, NavigationContext context)
    {
        await inner.ShowContentAsync(context);
    }
    private IEnumerable<IRegionIndicator> ResolveIndicators(NavigationContext context)
    {
        var regionIndicatorProviders = _serviceProvider.GetServices<IRegionIndicatorProvider>();

        foreach (var provider in regionIndicatorProviders)
        {
            if (provider.HasIndicator(context.RegionName))
            {
                yield return provider.GetIndicator(context.RegionName);
            }
        }
    }

    private static IInnerRegionIndicatorHost ResolveInnerIndicator(NavigationContext context)
    {
        if (context.IndicatorHost.Value is IInnerRegionIndicatorHost indicatorHost)
        {
            return indicatorHost;
        }
        throw new NavigationException(
           $"Region indicator for region '{context.RegionName}' is not set or is not an IInnerRegionIndicatorHost.");
    }
}
