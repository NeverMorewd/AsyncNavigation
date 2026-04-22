using AsyncNavigation.Core;

namespace AsyncNavigation.Abstractions;

public interface IIconResolver<T> where T : class
{
    /// <summary>
    /// Resolve an IconDescriptor to a platform-specific visual element.
    /// </summary>
    /// <param name="descriptor">Icon descriptor</param>
    /// <param name="size">Icon size in pixels</param>
    /// <returns>Platform-specific visual element</returns>
    T? Resolve(IconDescriptor descriptor, double size = 24);
}
