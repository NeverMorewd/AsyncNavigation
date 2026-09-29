namespace AsyncNavigation.Floating;

public sealed class FloatingWindowOptions
{
    public string? Title { get; init; }
    public double? Width { get; init; }
    public double? Height { get; init; }
    public bool Topmost { get; init; }

    internal FloatingWindowOptions WithDefaultTitle(string title) => new()
    {
        Title = Title ?? title,
        Width = Width,
        Height = Height,
        Topmost = Topmost
    };
}
