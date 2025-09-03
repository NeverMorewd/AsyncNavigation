using AsyncNavigation.Abstractions;
using AsyncNavigation.Core;
using Microsoft.Extensions.DependencyInjection;

namespace AsyncNavigation;

internal sealed class RegionIndicatorManager : IRegionIndicatorManager
{
    //private readonly Func<IInlineIndicator> _indicatorFactory;
    private IInlineIndicator? _singleton;
    private readonly IServiceProvider _serviceProvider;
    private readonly IRegionIndicatorProvider? _regionIndicatorProvider;

    public RegionIndicatorManager(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        //_indicatorFactory = _serviceProvider.GetRequiredService<IInlineIndicator>;
        _regionIndicatorProvider = _serviceProvider.GetService<IRegionIndicatorProvider>();
    }

    public void Setup(NavigationContext context, bool useSingleton)
    {
        //if (context.Indicator.IsSet)
        //{
        //    return;
        //}
        //var indicator = useSingleton
        //    ? _singleton ??= _indicatorFactory()
        //    : _indicatorFactory();

        //context.Indicator.Value = indicator;
    }

    public async Task ShowErrorAsync(NavigationContext context, Exception exception, bool throwIfNeed)
    {
        if (NavigationOptions.Default.EnableErrorIndicator)
        {
            await ResolveIndicator(context).ShowErrorAsync(context, exception);
        }
        else if (throwIfNeed)
        {
            throw exception;
        }
    }

    public async Task StartAsync(NavigationContext context, Task processTask, TimeSpan? delayTime = null)
    {
        if (NavigationOptions.Default.EnableLoadingIndicator)
        {
            if (delayTime.HasValue)
            {
                var delayTask = Task.Delay(delayTime.Value, context.CancellationToken);
                if (await Task.WhenAny(processTask, delayTask) == delayTask && !processTask.IsCompleted)
                {
                    await ResolveIndicator(context).ShowLoadingAsync(context);
                }
            }
            else
            {
                await ResolveIndicator(context).ShowLoadingAsync(context);
            }
        }
        await processTask;
        //context.Indicator.Value!.ShowContent(context);
    }

    private IRegionIndicator ResolveIndicator(NavigationContext context)
    {
        if (_regionIndicatorProvider != null && _regionIndicatorProvider.HasIndicator(context.RegionName))
        {
            return _regionIndicatorProvider.GetIndicator(context.RegionName);
        }
        //if (context.Indicator.IsSet)
        //    return context.Indicator.Value!;

        throw new NavigationException(
            $"Region indicator for region '{context.RegionName}' is not set or is not an IRegionIndicator.");
    }

    private IEnumerable<IRegionIndicator> ResolveIndicators(NavigationContext context)
    {
        List<IRegionIndicator> indicators = [];
        if (_regionIndicatorProvider != null && _regionIndicatorProvider.HasIndicator(context.RegionName))
        {
            var regionIndicator = _regionIndicatorProvider.GetIndicator(context.RegionName);
            indicators.Add(regionIndicator);
        }
        //if (context.Indicator.IsSet)
        //{
        //    var inlineIndicator = context.Indicator.Value!;
        //    indicators.Add(inlineIndicator);
        //}
        return indicators;
         
    }
}
