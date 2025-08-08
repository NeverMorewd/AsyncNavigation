using AsyncNavigation.Core;

namespace AsyncNavigation.Abstractions
{
    public interface IContentRegion<T> : IRegion
    {
        T ContentControl { get; }
    }
}
