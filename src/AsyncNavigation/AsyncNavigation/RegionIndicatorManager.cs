using AsyncNavigation.Abstractions;
using AsyncNavigation.Core;

namespace AsyncNavigation;

public class RegionIndicatorManager : IRegionIndicatorManager
{
    private readonly Func<IRegionIndicator> _indicatorFactory;
    private IRegionIndicator? _singleton;

    public RegionIndicatorManager(Func<IRegionIndicator> indicatorFactory)
    {
        _indicatorFactory = indicatorFactory;
    }

    public IRegionIndicator Setup(NavigationContext context, bool useSingleton)
    {
        var indicator = useSingleton
            ? (_singleton ??= _indicatorFactory())
            : _indicatorFactory();

        context.Indicator.Value = indicator;
        return indicator;
    }

    public Task ShowContentAsync(NavigationContext context, object content)
    {
        if (!NavigationOptions.Default.EnableLoadingIndicator)
            return Task.CompletedTask;

        GetIndicator(context).ShowContent(context, content);
        return Task.CompletedTask;
    }

    public Task ShowErrorAsync(NavigationContext context, Exception exception)
    {
        if (!NavigationOptions.Default.EnableErrorIndicator)
            throw exception;

        GetIndicator(context).ShowError(context, exception);
        return Task.CompletedTask;
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
                    GetIndicator(context).ShowLoading(context);
                }
            }
            else
            {
                GetIndicator(context).ShowLoading(context);
            }
        }
        await processTask;
        GetIndicator(context).ShowContent(context, context.Target.Value);
    }

    private static IRegionIndicator GetIndicator(NavigationContext context)
    {
        if (context.Indicator.IsSet && context.Indicator.Value is IRegionIndicator indicator)
            return indicator;

        throw new NavigationException(
            $"Region indicator for region '{context.RegionName}' is not set or is not an IRegionIndicator.");
    }
}
