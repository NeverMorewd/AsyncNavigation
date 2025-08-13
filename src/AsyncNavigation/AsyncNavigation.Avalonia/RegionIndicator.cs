using AsyncNavigation.Abstractions;
using AsyncNavigation.Core;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Microsoft.Extensions.DependencyInjection;

namespace AsyncNavigation.Avalonia;

public class RegionIndicator : IRegionIndicatorHost<ContentControl>
{
    private readonly IDataTemplate? _loadingTemplate;
    private readonly IDataTemplate? _errorTemplate;

    public RegionIndicator(IServiceProvider services)
    {
        Control = new ContentControl();

        if (NavigationOptions.Default.EnableLoadingIndicator)
            _loadingTemplate = services.GetKeyedService<IDataTemplate>(NavigationConstants.INDICATOR_LOADING_KEY);

        if (NavigationOptions.Default.EnableErrorIndicator)
            _errorTemplate = services.GetKeyedService<IDataTemplate>(NavigationConstants.INDICATOR_ERROR_KEY);
    }

    public ContentControl Control { get; }

    object IRegionIndicator.Control => Control;

    public void ShowLoading(NavigationContext context)
    {
        if (!NavigationOptions.Default.EnableLoadingIndicator) return;
        Control.Content = _loadingTemplate?.Build(context.WithStatus(NavigationStatus.InProgress));
    }

    public void ShowError(NavigationContext context, Exception exception)
    {
        if (!NavigationOptions.Default.EnableErrorIndicator) return;
        Control.Content = _errorTemplate?.Build(context.WithStatus(NavigationStatus.Failed, exception));
    }

    public void ShowContent(NavigationContext context, object? content)
    {
        Control.Content = content;
    }
}
