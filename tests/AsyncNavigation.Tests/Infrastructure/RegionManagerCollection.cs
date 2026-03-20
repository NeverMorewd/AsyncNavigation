using Xunit;

namespace AsyncNavigation.Tests.Infrastructure;

/// <summary>
/// Ensures all test classes that share a RegionManager (which is a process-wide singleton)
/// run sequentially and share a single ServiceFixture instance.
/// </summary>
[CollectionDefinition("RegionManagerCollection")]
public class RegionManagerCollection : ICollectionFixture<ServiceFixture> { }
