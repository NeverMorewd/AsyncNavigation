using AsyncNavigation.Abstractions;
using System.Windows;
using System.Windows.Threading;

namespace AsyncNavigation.Wpf;

/// <summary>
/// Extension methods that expose WPF-specific capabilities from an <see cref="IViewContext"/>.
/// </summary>
/// <remarks>
/// A ViewModel that receives an <see cref="IViewContext"/> via <see cref="IViewAware.OnViewAttached"/>
/// can access these members after adding a <c>using AsyncNavigation.Wpf;</c> directive:
/// <code>
/// private IViewContext? _viewContext;
///
/// public void OnViewAttached(IViewContext context) => _viewContext = context;
///
/// [ReactiveCommand]
/// private async Task DoWork()
/// {
///     if (_viewContext is null) return;
///     var window = _viewContext.GetWindow();
///     await _viewContext.GetDispatcher().InvokeAsync(() => window.Title = "Working…");
/// }
/// </code>
/// </remarks>
public static class WpfViewContextExtensions
{
    /// <summary>Gets the <see cref="Window"/> that contains the attached view.</summary>
    public static Window GetWindow(this IViewContext context)
        => ((ViewContext)context).Window;

    /// <summary>Gets the UI-thread <see cref="Dispatcher"/> for the window that contains the attached view.</summary>
    public static Dispatcher GetDispatcher(this IViewContext context)
        => ((ViewContext)context).Dispatcher;
}
