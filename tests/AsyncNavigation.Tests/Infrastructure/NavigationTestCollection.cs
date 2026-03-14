using Xunit;

namespace AsyncNavigation.Tests.Infrastructure;

/// <summary>
/// Ensures all navigation test classes share a single <see cref="ServiceFixture"/> instance,
/// because <see cref="AsyncNavigation.RegionManagerBase"/> enforces a process-wide singleton.
/// </summary>
[CollectionDefinition(Name)]
public class NavigationTestCollection : ICollectionFixture<ServiceFixture>
{
    public const string Name = "Navigation";
}
