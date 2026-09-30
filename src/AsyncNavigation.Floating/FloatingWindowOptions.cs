namespace AsyncNavigation.Floating;

public sealed class FloatingWindowOptions
{
    public string? Title { get; init; }
    public double? Width { get; init; }
    public double? Height { get; init; }
    public bool Topmost { get; init; }

    /// <summary>
    /// Whether the floating host should render its own "Dock to region" button.
    /// Set this to <see langword="false"/> when the floated content provides its own
    /// dock/restore affordance and drives it through <see cref="IFloatingViewSession.RestoreAsync"/>.
    /// </summary>
    public bool ShowDockButton { get; init; } = true;

    internal FloatingWindowOptions WithDefaultTitle(string title) => new()
    {
        Title = Title ?? title,
        Width = Width,
        Height = Height,
        Topmost = Topmost,
        ShowDockButton = ShowDockButton
    };
}
