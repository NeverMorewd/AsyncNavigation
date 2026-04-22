using AsyncNavigation.Core;

namespace AsyncNavigation.Abstractions;

public interface INavigationMetadata
{
    IconDescriptor Icon { get; }
    string Title { get; }
}
