namespace AsyncNavigation.Abstractions;

/// <summary>
/// Represents the view context provided to a view-aware view model when its associated view is attached to the visual tree.
/// Platform-specific capabilities (e.g., StorageProvider, Clipboard for Avalonia; Window for WPF)
/// are exposed via extension properties defined in each platform package.
/// </summary>
public interface IViewContext { }
