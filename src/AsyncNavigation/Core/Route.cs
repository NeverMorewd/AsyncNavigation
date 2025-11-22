namespace AsyncNavigation.Core;

public class Route
{
    public required string Path { get; init; }
    public required IReadOnlyList<NavigationTarget> Steps { get; init; }
    public NavigationTarget? Fallback { get; init; }
    internal string[] Segments { get; set; } = [];
}
