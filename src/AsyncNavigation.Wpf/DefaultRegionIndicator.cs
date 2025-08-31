using AsyncNavigation.Abstractions;
using AsyncNavigation.Core;
using Microsoft.Extensions.DependencyInjection;
using System.Windows.Controls;

namespace AsyncNavigation.Wpf;

internal sealed class DefaultSelfIndicator : ISelfIndicator
{
    private readonly IndicatorDataTemplate? _loadingTemplate;
    private readonly IndicatorDataTemplate? _errorTemplate;
    private readonly ContentControl _indicatorControl;
    public DefaultSelfIndicator(IServiceProvider services)
    {
        _indicatorControl = new ContentControl();

        if (NavigationOptions.Default.EnableLoadingIndicator)
            _loadingTemplate = services.GetRequiredKeyedService<IndicatorDataTemplate>(NavigationConstants.INDICATOR_LOADING_KEY);

        if (NavigationOptions.Default.EnableErrorIndicator)
            _errorTemplate = services.GetRequiredKeyedService<IndicatorDataTemplate>(NavigationConstants.INDICATOR_ERROR_KEY);
    }

    object ISelfIndicator.IndicatorControl => _indicatorControl;

    public void ShowLoading(NavigationContext context)
    {
        if (_loadingTemplate == null)
            throw new NavigationException($"Failed to resolve loading template (key: {NavigationConstants.INDICATOR_LOADING_KEY}) from IServiceProvider. " +
             "Please ensure it is registered before calling ShowLoading().");
        _indicatorControl.Content = _loadingTemplate?.Build(context.WithStatus(NavigationStatus.InProgress));
    }

    public void ShowError(NavigationContext context, Exception? exception)
    {
        if (_errorTemplate == null)
            throw new NavigationException($"Failed to resolve error template (key: {NavigationConstants.INDICATOR_ERROR_KEY}) from IServiceProvider. " +
             "Please ensure it is registered before calling ShowError().");

        if (exception != null)
        {
            context = context.WithStatus(NavigationStatus.Failed, exception);
        }

        _indicatorControl.Content = _errorTemplate?.Build(context);
    }

    public void ShowContent(NavigationContext context)
    {
        _indicatorControl.Content = context.Target.Value;
    }


    Task IRegionIndicator.ShowErrorAsync(NavigationContext context, Exception? innerException)
    {
        ShowError(context, innerException);
        return Task.CompletedTask;
    }

    Task IRegionIndicator.ShowLoadingAsync(NavigationContext context)
    {
        ShowLoading(context);
        return Task.CompletedTask;
    }
}
