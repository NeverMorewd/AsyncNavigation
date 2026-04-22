namespace AsyncNavigation.Core;

public class IconDescriptor
{
    public IconKind Kind { get; init; }
    public string Value { get; init; } = string.Empty;

    public static IconDescriptor FromFile(string path) =>
        new() { Kind = IconKind.FilePath, Value = path };

    public static IconDescriptor FromPathData(string pathData) =>
        new() { Kind = IconKind.PathData, Value = pathData };

    public static IconDescriptor FromIconFont(string name) =>
        new() { Kind = IconKind.IconFont, Value = name };

    public static IconDescriptor FromResourceKey(string resourceKey) =>
        new() { Kind = IconKind.ResourceKey, Value = resourceKey };

    public static readonly IconDescriptor None = new() { Kind = IconKind.None };
}
