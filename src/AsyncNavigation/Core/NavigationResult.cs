namespace AsyncNavigation.Core;

public class NavigationResult
{
    private NavigationResult()
    {
    }
    public bool IsFailed => Status == NavigationStatus.Failed;
    public bool IsCancelled => Status == NavigationStatus.Cancelled;
    public bool IsSuccessful => Status == NavigationStatus.Succeeded;
    public NavigationStatus Status { get; private init; }
    public Exception? Exception { get; private init; }
    public TimeSpan Duration { get; private init; }
    public NavigationContext? NavigationContext { get; private init; }

    public static NavigationResult Success(TimeSpan duration, NavigationContext? navigationContext = null)
    {
        if (navigationContext != null)
        {
            navigationContext.Duration = duration;
            navigationContext.WithStatus(NavigationStatus.Succeeded);
            return new NavigationResult
            {
                NavigationContext = navigationContext,
                Status = navigationContext.Status,
                Exception = navigationContext.Errors,
                Duration = navigationContext.Duration.GetValueOrDefault()
            };
        }

        return new NavigationResult
        {
            Status = NavigationStatus.Succeeded,
            Exception = null,
            Duration = duration,
            NavigationContext = null
        };
    }

    public static NavigationResult Failure(Exception exception, TimeSpan duration, NavigationContext? navigationContext = null)
    {
        if (navigationContext != null)
        {
            navigationContext.Duration = duration;
            return new NavigationResult
            {
                NavigationContext = navigationContext.WithStatus(NavigationStatus.Failed, exception),
                Status = navigationContext.Status,
                Exception = navigationContext.Errors,
                Duration = navigationContext.Duration.GetValueOrDefault()
            };
        }

        return new NavigationResult
        {
            Status = NavigationStatus.Failed,
            Exception = exception,
            Duration = duration,
            NavigationContext = null
        };
    }

    public static NavigationResult Cancelled(TimeSpan duration, NavigationContext? navigationContext = null)
    {
        if (navigationContext != null)
        {
            navigationContext.Duration = duration;
            return new NavigationResult
            {
                NavigationContext = navigationContext.WithStatus(NavigationStatus.Cancelled),
                Status = navigationContext.Status,
                Exception = navigationContext.Errors,
                Duration = navigationContext.Duration.GetValueOrDefault()
            };
        }

        return new NavigationResult
        {
            Status = NavigationStatus.Cancelled,
            Exception = null,
            Duration = duration,
            NavigationContext = null
        };
    }
}
