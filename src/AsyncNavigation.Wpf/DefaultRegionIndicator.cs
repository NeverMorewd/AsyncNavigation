using AsyncNavigation.Abstractions;
using AsyncNavigation.Core;
using Microsoft.Extensions.DependencyInjection;
using System.Windows.Controls;

namespace AsyncNavigation.Wpf;

internal sealed class DefaultRegionIndicator : IRegionIndicator
{
    private readonly IndicatorDataTemplate? _loadingTemplate;
    private readonly IndicatorDataTemplate? _errorTemplate;
    private readonly ContentControl _indicatorControl;
    public DefaultRegionIndicator(IServiceProvider services)
    {
        _indicatorControl = new ContentControl();

        if (NavigationOptions.Default.EnableLoadingIndicator)
            _loadingTemplate = services.GetRequiredKeyedService<IndicatorDataTemplate>(NavigationConstants.INDICATOR_LOADING_KEY);

        if (NavigationOptions.Default.EnableErrorIndicator)
            _errorTemplate = services.GetRequiredKeyedService<IndicatorDataTemplate>(NavigationConstants.INDICATOR_ERROR_KEY);
    }

    object IRegionIndicator.IndicatorControl => _indicatorControl;

    public void ShowLoading(NavigationContext context)
    {
        if (_loadingTemplate == null)
            throw new NavigationException($"Failed to resolve loading template (key: {NavigationConstants.INDICATOR_LOADING_KEY}) from IServiceProvider. " +
             "Please ensure it is registered before calling ShowLoading().");
        _indicatorControl.Content = _loadingTemplate?.Build(context.WithStatus(NavigationStatus.InProgress));
    }

    public void ShowError(NavigationContext context, Exception exception)
    {
        if (_errorTemplate == null)
            throw new NavigationException($"Failed to resolve error template (key: {NavigationConstants.INDICATOR_ERROR_KEY}) from IServiceProvider. " +
             "Please ensure it is registered before calling ShowError().");
        _indicatorControl.Content = _errorTemplate?.Build(context.WithStatus(NavigationStatus.Failed, exception));
    }

    public void ShowContent(NavigationContext context, object? content)
    {
        _indicatorControl.Content = content;
    }
}
