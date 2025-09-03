using AsyncNavigation.Abstractions;
using AsyncNavigation.Core;
using System.Collections.Concurrent;
using System.Text;

namespace AsyncNavigation;


/// <summary>
/// Encapsulates information for a single navigation request.
/// </summary>
public partial class NavigationContext
{
    private readonly ConcurrentBag<Exception> _errors = [];
    /// <summary>
    /// Gets the name of the target region.
    /// </summary>
    public required string RegionName { get; init; }

    /// <summary>
    /// Gets the name of the target view.
    /// </summary>
    public required string ViewName { get; init; }

    public CancellationToken CancellationToken { get; internal set; } = default;

    /// <summary>
    /// Gets the navigation parameters passed to the target view.
    /// </summary>
    public INavigationParameters? Parameters { get; internal set; } = null;


    public ImmutableProperty<IView> Source { get; } = new();
    public ImmutableProperty<IView> Target { get; } = new();
    //public ImmutableProperty<IInlineIndicator> Indicator { get; } = new();

    /// <summary>
    /// Gets the timestamp when navigation was initiated.
    /// </summary>
    public DateTime NavigationTime { get; init; } = DateTime.UtcNow;

    /// <summary>
    /// Gets a value indicating whether this is a back navigation operation.
    /// </summary>
    public bool IsBackNavigation { get; internal set; }

    /// <summary>
    /// Gets a value indicating whether this is a forward navigation operation.
    /// </summary>
    public bool IsForwordNavigation { get; internal set; }
    /// <summary>
    /// Gets the current status of the navigation operation.
    /// </summary>
    private NavigationStatus _status;
    public NavigationStatus Status 
    {
        get => _status;
        set
        {
            if (_status != value)
            {
                _status = value;
                OnPropertyChanged();
            }
        }
    }

    /// <summary>
    /// Gets the error that occurred during navigation, if any.
    /// </summary>
    public AggregateException? Errors => new(_errors);

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
        NavigationTime = DateTime.MinValue,
        IsBackNavigation = false,
        Status = NavigationStatus.Failed,
        NavigationId = Guid.Empty,
        Duration = null,
        CancellationToken = default,
    };

    public NavigationContext WithStatus(NavigationStatus newStatus, params Exception[] errors)
    {
        if (IsCompleted)
            throw new InvalidOperationException("Cannot change status after navigation is completed.");

        Status = newStatus;
        Duration = DateTime.UtcNow - NavigationTime;
        return WithErrors(errors);
    }

    public NavigationContext WithParameter(string key, object value)
    {
        if (IsCompleted)
            throw new InvalidOperationException("Cannot add parameters after navigation is completed.");

        Parameters ??= new NavigationParameters();
        Parameters.Add(key, value);
        return this;
    }

    public NavigationContext WithParameters(IEnumerable<KeyValuePair<string, object>> parameters)
    {
        if (IsCompleted)
            throw new InvalidOperationException("Cannot add parameters after navigation is completed.");

        Parameters ??= new NavigationParameters();
        Parameters.AddRange(parameters);
        return this;
    }

    public NavigationContext WithErrors(params Exception[] exceptions)
    {
        if (IsCompleted)
            throw new InvalidOperationException("Cannot add errors after navigation is completed.");

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
        var sb = new StringBuilder();

        sb.AppendFormat("Navigation[{0:N}]: {1} in {2} - {3}", NavigationId, ViewName, RegionName, Status);

        if (IsBackNavigation)
        {
            sb.Append(" (Back)");
        }

        sb.AppendFormat(" - {0}", Duration?.ToString() ?? "N/A");

        if (Errors?.InnerExceptions is { Count: > 0 } exceptions)
        {
            sb.AppendFormat(" (Errors: {0})", exceptions.Count);
            sb.AppendLine();
            foreach (var ex in exceptions)
            {
                sb.AppendLine(ex.ToString());
            }
        }

        return sb.ToString();
    }


    public override bool Equals(object? obj)
    {
        if (Target.IsSet && obj is NavigationContext context && context.Target.IsSet)
        {
            return ReferenceEquals(Target.Value, context.Target.Value);
        }
        return base.Equals(obj);
    }
    public override int GetHashCode()
    {
        if (Target.IsSet)
        {
            return Target.Value!.GetHashCode();
        }
        return base.GetHashCode();
    }
}
