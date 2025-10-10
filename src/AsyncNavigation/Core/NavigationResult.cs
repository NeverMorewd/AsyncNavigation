using System.Text;

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

    public static NavigationResult Success(NavigationContext navigationContext)
    {
        navigationContext.UpdateStatus(NavigationStatus.Succeeded);
        return new NavigationResult
        {
            NavigationContext = navigationContext,
            Status = navigationContext.Status,
            Exception = navigationContext.Errors,
            Duration = navigationContext.Duration.GetValueOrDefault()
        };
    }
    public static NavigationResult Success(TimeSpan duration)
    {
        return new NavigationResult
        {
            Status = NavigationStatus.Succeeded,
            Exception = null,
            Duration = duration,
            NavigationContext = null
        };
    }

    public static NavigationResult Failure(Exception exception, NavigationContext navigationContext)
    {
        navigationContext.UpdateStatus(NavigationStatus.Failed, exception);
        return new NavigationResult
        {
            NavigationContext = navigationContext,
            Status = navigationContext.Status,
            Exception = navigationContext.Errors,
            Duration = navigationContext.Duration.GetValueOrDefault()
        };
    }

    public static NavigationResult Failure(Exception exception)
    {
        return new NavigationResult
        {
            Status = NavigationStatus.Failed,
            Exception = exception,
            Duration = TimeSpan.Zero,
            NavigationContext = null
        };
    }

    public static NavigationResult Cancelled(NavigationContext navigationContext)
    {
        navigationContext.UpdateStatus(NavigationStatus.Cancelled);

        return new NavigationResult
        {
            NavigationContext = navigationContext,
            Status = navigationContext.Status,
            Exception = navigationContext.Errors,
            Duration = navigationContext.Duration.GetValueOrDefault()
        };
    }

    public static NavigationResult Cancelled()
    {
        return new NavigationResult
        {
            Status = NavigationStatus.Cancelled,
            Exception = null,
            Duration = TimeSpan.Zero,
            NavigationContext = null
        };
    }

    public override string ToString()
    {
        var sb = new StringBuilder();
        sb.AppendLine($"Status: {Status}");
        sb.AppendLine($"Duration: {Duration.TotalMilliseconds:F2} ms");

        if (Exception != null)
        {
            sb.AppendLine($"Exception: {Exception.GetType().Name} - {Exception.Message}");
        }

        if (NavigationContext != null)
        {
            sb.AppendLine($"NavigationContext: {NavigationContext}");
        }

        return sb.ToString().TrimEnd();
    }


}
