using AsyncNavigation.Abstractions;
using AsyncNavigation.Core;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Concurrent;

namespace AsyncNavigation;

internal sealed class RegionIndicatorManager : IRegionIndicatorManager
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IInnerRegionIndicatorHost _innerRegionIndicatorHost;
    private readonly ConcurrentDictionary<NavigationContext, (IInnerRegionIndicatorHost Inner, IReadOnlyList<IRegionIndicator> Others)> _cachedIndicators;

    public RegionIndicatorManager(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        _cachedIndicators = new ConcurrentDictionary<NavigationContext, (IInnerRegionIndicatorHost, IReadOnlyList<IRegionIndicator>)>();
        _innerRegionIndicatorHost = _serviceProvider.GetRequiredService<IInnerRegionIndicatorHost>();
    }

    public void Setup(NavigationContext context, bool useSingleton)
    {
        if (context.IndicatorHost.IsSet) return;

        context.IndicatorHost.Value = useSingleton
            ? _innerRegionIndicatorHost
            : _serviceProvider.GetRequiredService<IInnerRegionIndicatorHost>();
    }

    public Task ShowErrorAsync(NavigationContext context, Exception exception)
    {
        //var (Inner, Others) = _cachedIndicators.GetOrAdd(context, _ => ResolveRegionIndicators(context));
        var (Inner, Others) = ResolveRegionIndicators(context);
        return ShowErrorCore(Inner, Others, context, exception);
    }

    public async Task StartAsync(NavigationContext context, Task processTask, TimeSpan? delayTime = null)
    {
        //var (Inner, Others) = _cachedIndicators.GetOrAdd(context, _ => ResolveRegionIndicators(context));
        var (Inner, Others) = ResolveRegionIndicators(context);
        if (await ShouldShowLoading(context, processTask, delayTime))
        {
            await ShowLoadingCore(Inner, Others, context);
        }

        try
        {
            await processTask;
        }
        catch (OperationCanceledException) when (context.CancellationToken.IsCancellationRequested)
        {
            await OnCancelledCore(Inner, Others, context);
            throw;
        }

        await OnLoadedCore(Inner, Others, context);
        await ShowContentCore(Inner, context);
    }
    private static IEnumerable<IRegionIndicator> AllIndicators(IRegionIndicator inner, IEnumerable<IRegionIndicator> others) =>
        others?.Append(inner) ?? [inner];

    private static async Task<bool> ShouldShowLoading(NavigationContext context, Task processTask, TimeSpan? delayTime)
    {
        if (!delayTime.HasValue)
            return true;

        var delayTask = Task.Delay(delayTime.Value, context.CancellationToken);
        return await Task.WhenAny(processTask, delayTask) == delayTask && !processTask.IsCompleted;
    }

    private static async Task ShowErrorCore(IRegionIndicator inner, IEnumerable<IRegionIndicator> others, NavigationContext context, Exception exception)
    {
        await Task.WhenAll(AllIndicators(inner, others).Select(indicator => indicator.ShowErrorAsync(context, exception)));
    }

    private static async Task ShowLoadingCore(IRegionIndicator inner, IEnumerable<IRegionIndicator> others, NavigationContext context)
    {
        await Task.WhenAll(AllIndicators(inner, others).Select(indicator => indicator.ShowLoadingAsync(context)));
    }

    private static async Task OnLoadedCore(IRegionIndicator inner, IEnumerable<IRegionIndicator> others, NavigationContext context)
    {
        await Task.WhenAll(AllIndicators(inner, others).Select(indicator => indicator.OnLoadedAsync(context)));
    }

    private static async Task OnCancelledCore(IRegionIndicator inner, IEnumerable<IRegionIndicator> others, NavigationContext context)
    {
        await Task.WhenAll(AllIndicators(inner, others).Select(indicator => indicator.OnCancelledAsync(context)));
    }

    private static Task ShowContentCore(IInnerRegionIndicatorHost inner, NavigationContext context) =>
        inner.ShowContentAsync(context);

    private (IInnerRegionIndicatorHost Inner, IReadOnlyList<IRegionIndicator> Others) ResolveRegionIndicators(NavigationContext context)
    {
        var inner = ResolveInnerIndicator(context);
        var regionIndicators = ResolveIndicators(context);
        return (inner, regionIndicators);
    }

    private List<IRegionIndicator> ResolveIndicators(NavigationContext context)
    {
        var result = new List<IRegionIndicator>();
        foreach (var provider in _serviceProvider.GetServices<IRegionIndicatorProvider>())
        {
            if (provider.HasIndicator(context.RegionName))
            {
                result.Add(provider.GetIndicator(context.RegionName));
            }
        }
        return result;
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
