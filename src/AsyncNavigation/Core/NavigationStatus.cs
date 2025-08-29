namespace AsyncNavigation.Core;

/// <summary>
/// Represents the status of a navigation operation.
/// </summary>
public enum NavigationStatus
{
    /// <summary>
    /// Navigation has not started yet.
    /// </summary>
    Pending,

    /// <summary>
    /// Navigation is currently in progress.
    /// </summary>
    InProgress,

    /// <summary>
    /// Navigation completed successfully.
    /// </summary>
    Succeeded,

    /// <summary>
    /// Navigation was cancelled by user or system.
    /// </summary>
    Cancelled,

    /// <summary>
    /// Navigation failed due to an error.
    /// </summary>
    Failed
}
