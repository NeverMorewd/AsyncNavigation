namespace AsyncNavigation.Core;

public class NavigationResult
{
    public bool IsSuccess { get; }
    public string? ErrorMessage { get; }
    public Exception? Exception { get; }

    private NavigationResult(bool isSuccess, string? errorMessage = null, Exception? exception = null)
    {
        IsSuccess = isSuccess;
        ErrorMessage = errorMessage;
        Exception = exception;
    }

    public static NavigationResult Successful() => new(true);
    public static NavigationResult Failed(string errorMessage, Exception? exception = null) => new(false, errorMessage, exception);
    public static NavigationResult Cancelled() => new(false, "Navigation was cancelled");
}