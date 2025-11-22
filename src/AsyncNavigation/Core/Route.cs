namespace AsyncNavigation.Core;

public record Route
{
    public required string Path { get; init; }
    public required IReadOnlyList<NavigationTarget> Targets { get; init; }
    public NavigationTarget? Fallback { get; internal set; }
    internal string[] Segments { get; set; } = [];
}
