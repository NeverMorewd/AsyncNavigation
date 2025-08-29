namespace AsyncNavigation.Core;

public class AsyncNavigationEventArgs : AsyncEventArgs
{
    public NavigationContext NavigationContext { get; }
    public AsyncNavigationEventArgs(NavigationContext navigationContext) : base(navigationContext.CancellationToken)
    {
        NavigationContext = navigationContext;
    }
}
