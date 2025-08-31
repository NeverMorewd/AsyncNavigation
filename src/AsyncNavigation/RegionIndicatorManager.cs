using AsyncNavigation.Abstractions;
using AsyncNavigation.Core;
using Microsoft.Extensions.DependencyInjection;

namespace AsyncNavigation;

internal sealed class RegionIndicatorManager : IRegionIndicatorManager
{
    private readonly Func<ISelfIndicator> _indicatorFactory;
    private ISelfIndicator? _singleton;
    private readonly IServiceProvider _serviceProvider;
    private readonly IRegionIndicatorProvider? _regionIndicatorProvider;
    private readonly IIndicatorProvider? _indicatiorProvider;

    public RegionIndicatorManager(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        _indicatorFactory = _serviceProvider.GetRequiredService<ISelfIndicator>;
        _regionIndicatorProvider = _serviceProvider.GetService<IRegionIndicatorProvider>();
        _indicatiorProvider = _serviceProvider.GetService<IIndicatorProvider>();
    }

    public void Setup(NavigationContext context, bool useSingleton)
    {
        if (context.Indicator.IsSet)
        {
            return;
        }
        var indicator = useSingleton
            ? _singleton ??= _indicatorFactory()
            : _indicatorFactory();

        context.Indicator.Value = indicator;
    }

    public async Task ShowErrorAsync(NavigationContext context, Exception exception, bool throwIfNeed)
    {
        if (NavigationOptions.Default.EnableErrorIndicator)
        {
            await GetIndicator(context).ShowErrorAsync(context, exception);
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
                    await GetIndicator(context).ShowLoadingAsync(context);
                }
            }
            else
            {
                await GetIndicator(context).ShowLoadingAsync(context);
            }
        }
        await processTask;
        context.Indicator.Value!.ShowContent(context);
    }

    private IRegionIndicator GetIndicator(NavigationContext context)
    {
        if (_regionIndicatorProvider != null && _regionIndicatorProvider.HasIndicator(context.RegionName))
        {
            return _regionIndicatorProvider.GetIndicator(context.RegionName);
        }
        if (_indicatiorProvider != null && _indicatiorProvider.HasIndicator())
        {
            return _indicatiorProvider.GetIndicator();
        }
        if (context.Indicator.IsSet)
            return context.Indicator.Value!;

        throw new NavigationException(
            $"Region indicator for region '{context.RegionName}' is not set or is not an IRegionIndicator.");
    }
}
