using AsyncNavigation.Abstractions;
using System.Windows;

namespace AsyncNavigation.Wpf;

public interface IInnerIndicatorProvider : IInnerIndicatorProviderBase
{
    UIElement GetErrorIndicator(NavigationContext navigationContext);
    UIElement GetLoadingIndicator(NavigationContext navigationContext);
}
