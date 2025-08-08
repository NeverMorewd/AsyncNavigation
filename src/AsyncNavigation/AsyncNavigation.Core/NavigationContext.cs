using System.Collections.Concurrent;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace AsyncNavigation.Core;


/// <summary>
/// Encapsulates information for a single navigation request.
/// </summary>
public class NavigationContext
{
    private readonly ConcurrentBag<Exception> _errors = [];
    /// <summary>
    /// Gets the name of the region where navigation occurs.
    /// </summary>
    public required string RegionName { get; init; }

    /// <summary>
    /// Gets the name of the target view.
    /// </summary>
    public required string ViewName { get; init; }

    public required CancellationToken CancellationToken { get; init; } = default;

    /// <summary>
    /// Gets the navigation parameters passed to the target view.
    /// </summary>
    public NavigationParameters Parameters { get; init; } = [];

    ///// <summary>
    ///// Gets the source view that initiated the navigation.
    ///// </summary>
    public object? Source { get; set; }

    ///// <summary>
    ///// Gets the target view instance after navigation.
    ///// </summary>
    public object? Target { get; set; }

    /// <summary>
    /// Gets the timestamp when navigation was initiated.
    /// </summary>
    public DateTime NavigationTime { get; init; } = DateTime.UtcNow;

    /// <summary>
    /// Gets a value indicating whether this is a back navigation operation.
    /// </summary>
    public bool IsBackNavigation { get; set; }

    /// <summary>
    /// Gets the current status of the navigation operation.
    /// </summary>
    public NavigationStatus Status { get; set; } = NavigationStatus.Pending;

    /// <summary>
    /// Gets the error that occurred during navigation, if any.
    /// </summary>
    public IReadOnlyCollection<Exception>? Errors => _errors;

    /// <summary>
    /// Gets a unique identifier for this navigation context.
    /// </summary>
    public Guid NavigationId { get; init; } = Guid.NewGuid();

    /// <summary>
    /// Gets the duration of the navigation operation.
    /// Only meaningful when Status is Succeeded, Failed, or Cancelled.
    /// </summary>
    public TimeSpan? Duration { get; set; }

    /// <summary>
    /// Gets additional metadata associated with this navigation.
    /// </summary>
    //public IDictionary<string, object?> Metadata { get; init; } = new Dictionary<string, object?>();

    /// <summary>
    /// Gets a value indicating whether the navigation completed successfully.
    /// </summary>
    public bool IsSuccessful => Status == NavigationStatus.Succeeded;

    /// <summary>
    /// Gets a value indicating whether the navigation failed.
    /// </summary>
    public bool IsFailed => Status == NavigationStatus.Failed;

    /// <summary>
    /// Gets a value indicating whether the navigation was cancelled.
    /// </summary>
    public bool IsCancelled => Status == NavigationStatus.Cancelled;

    /// <summary>
    /// Gets a value indicating whether the navigation is still in progress.
    /// </summary>
    public bool IsInProgress => Status == NavigationStatus.InProgress;

    /// <summary>
    /// Gets a value indicating whether the navigation has completed (either successfully, failed, or cancelled).
    /// </summary>
    public bool IsCompleted => Status is NavigationStatus.Succeeded or NavigationStatus.Failed or NavigationStatus.Cancelled;

    /// <summary>
    /// Represents an empty navigation context.
    /// </summary>
    public static readonly NavigationContext Empty = new()
    {
        RegionName = string.Empty,
        ViewName = string.Empty,
        Source = null,
        Target = null,
        NavigationTime = DateTime.MinValue,
        IsBackNavigation = false,
        Status = NavigationStatus.Failed,
        NavigationId = Guid.Empty,
        Duration = null,
        CancellationToken = default,
        //Metadata = new Dictionary<string, object?>()
    };

    /// <summary>
    /// Creates a new NavigationContext with updated status.
    /// </summary>
    /// <param name="newStatus">The new navigation status.</param>
    /// <param name="error">Optional error information.</param>
    /// <param name="duration">Optional duration information.</param>
    /// <returns>A new NavigationContext with the updated status.</returns>
    public NavigationContext WithStatus(NavigationStatus newStatus,
        params Exception[] errors)
    {
        Status = newStatus;
        Duration = DateTime.UtcNow - NavigationTime;
        return WithErrors(errors);
    }
    public NavigationContext WithIsBackNavigation(bool isBack)
    {
        IsBackNavigation = isBack;
        return this;
    }
    /// <summary>
    /// Creates a new NavigationContext with Parameters.
    /// </summary>
    /// <param name="key">The parameter key.</param>
    /// <param name="value">The parameter value.</param>
    /// <returns>A new NavigationContext with the added parameter.</returns>
    public NavigationContext WithParameter(string key, object value)
    {
        Parameters.Add(key, value);
        return this;
    }
    public NavigationContext WithParameters(IEnumerable<KeyValuePair<string, object>> parameters)
    {
        Parameters.AddRange(parameters);
        return this;
    }
    public NavigationContext WithErrors(params Exception[] exceptions)
    {
        foreach (var ex in exceptions)
        {
            _errors.Add(ex);
        }
        return this;
    }
    /// <summary>
    /// Returns a string representation of the navigation context.
    /// </summary>
    public override string ToString()
    {
        var backIndicator = IsBackNavigation ? " (Back)" : "";
        var errors = Errors?.Count > 0 ? $" (Errors: {Errors.Count}) {Environment.NewLine} {string.Join(Environment.NewLine,Errors.Select(e=>e.ToString()))}" : "";
        return $"Navigation[{NavigationId:N}]: {ViewName} in {RegionName} - {Status}{backIndicator} - {Duration} {errors}";
    }
}
