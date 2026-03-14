namespace Sample.Common.Messages;

/// <summary>
/// Broadcast by <see cref="AViewModel"/> each time its view is attached by the navigation framework.
/// Any subscriber (VM-to-VM communication) receives this without a direct reference to AViewModel.
/// </summary>
public sealed class ViewAttachedMessage(string source, string platformContextName)
{
    /// <summary>Display name of the ViewModel that was attached (e.g. "AViewModel:3").</summary>
    public string Source { get; } = source;

    /// <summary>Runtime type name of the platform-specific <c>IViewContext</c> (e.g. "ViewContext").</summary>
    public string PlatformContextName { get; } = platformContextName;

    /// <summary>UTC timestamp of when the view was attached.</summary>
    public DateTimeOffset Timestamp { get; } = DateTimeOffset.Now;
}
