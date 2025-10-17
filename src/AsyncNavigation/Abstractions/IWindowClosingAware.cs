using AsyncNavigation.Core;

namespace AsyncNavigation.Abstractions;

public interface IWindowClosingAware
{
    event EventHandler<WindowClosingEventArgs>? Closing;
}
