namespace AsyncNavigation.Core
{
    public class NavigationEventArgs : AsyncEventArgs
    {
        public NavigationEventArgs(NavigationContext navigationContext) : base(navigationContext.CancellationToken)
        {
            NavigationContext = navigationContext;
        }
        public NavigationContext NavigationContext { get; }
    }
    public class ViewActivatedEventArgs<T> : EventArgs
    {
        public T ViewModel { get; }
        public ViewActivatedEventArgs(T viewModel)
        {
            ViewModel = viewModel;
        }
    }
    public class ViewDeactivatedEventArgs<T> : EventArgs
    {
        public T ViewModel { get; }
        public ViewDeactivatedEventArgs(T viewModel)
        {
            ViewModel = viewModel;
        }
    }
    public class ViewAddedEventArgs<T> : EventArgs
    {
        public T ViewModel { get; }
        public ViewAddedEventArgs(T viewModel)
        {
            ViewModel = viewModel;
        }
    }
    public class ViewRemovedEventArgs<T> : EventArgs
    {
        public T ViewModel { get; }
        public ViewRemovedEventArgs(T viewModel)
        {
            ViewModel = viewModel;
        }
    }
    public class NavigationFailedEventArgs : EventArgs
    {
        public NavigationContext NavigationContext { get; }
        public Exception Exception { get; }
        public bool Handled { get; set; }

        public NavigationFailedEventArgs(NavigationContext context, Exception exception)
        {
            NavigationContext = context;
            Exception = exception;
        }
    }
}
