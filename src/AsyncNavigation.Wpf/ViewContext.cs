using AsyncNavigation.Abstractions;
using System.Windows;
using System.Windows.Threading;

namespace AsyncNavigation.Wpf;

internal sealed class ViewContext : IViewContext
{
    public ViewContext(Window window)
    {
        Window = window;
    }

    public Window Window { get; }
    public Dispatcher Dispatcher => Window.Dispatcher;
}
