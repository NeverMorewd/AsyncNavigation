namespace AsyncNavigation.Core;

public class NavigationResult
{
    public bool IsSuccess { get; init; }
    public NavigationStatus Status { get; init; }
    public Exception? Exception { get; init; }
    public TimeSpan Duration { get; init; }
    public static NavigationResult Success(TimeSpan duration) => new() { IsSuccess = true, Status = NavigationStatus.Succeeded, Duration = duration };
    public static NavigationResult Failure(Exception exception, TimeSpan duration) => new() { IsSuccess = false, Status = NavigationStatus.Failed, Exception = exception, Duration = duration };
    public static NavigationResult Cancelled(TimeSpan duration) => new() { IsSuccess = false, Status = NavigationStatus.Cancelled, Duration = duration };
}