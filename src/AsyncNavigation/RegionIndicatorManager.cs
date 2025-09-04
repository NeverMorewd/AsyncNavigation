using AsyncNavigation.Abstractions;
using AsyncNavigation.Core;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace AsyncNavigation;

internal sealed class RegionIndicatorManager : IRegionIndicatorManager
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IInnerRegionIndicatorHost _innerRegionIndicatorHost;

    public RegionIndicatorManager(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
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
        return ShowErrorCore(context, exception);
    }

    public async Task StartAsync(NavigationContext context, Task processTask, TimeSpan? delayTime = null)
    {
        if (delayTime.HasValue)
        {
            var delayTask = Task.Delay(delayTime.Value, context.CancellationToken);
            if (await Task.WhenAny(processTask, delayTask) == delayTask && !processTask.IsCompleted)
            {
                await ShowLoadingCore(context);
            }
        }
        else
        {
            await ShowLoadingCore(context);
        }
        await processTask;
        await ShowContentCore(context);
    }
    private async Task ShowErrorCore(NavigationContext context, Exception exception)
    {
        await ResolveInnerIndicator(context).ShowErrorAsync(context, exception);
        var regionIndicators = ResolveIndicators(context);
        foreach (var indicator in regionIndicators)
        {
            await indicator.ShowErrorAsync(context, exception);
        }
    }
    private async Task ShowLoadingCore(NavigationContext context)
    {
        await ResolveInnerIndicator(context).ShowLoadingAsync(context);
        var regionIndicators = ResolveIndicators(context);
        foreach (var indicator in regionIndicators)
        {
            await indicator.ShowLoadingAsync(context);
        }
    }
    private static async Task ShowContentCore(NavigationContext context)
    {
        await ResolveInnerIndicator(context).ShowContentAsync(context);
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
