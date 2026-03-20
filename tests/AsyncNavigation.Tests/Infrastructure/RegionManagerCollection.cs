using Xunit;

namespace AsyncNavigation.Tests.Infrastructure;

/// <summary>
/// Ensures all test classes that create a RegionManager (which is a process-wide singleton)
/// run sequentially and never in parallel with each other.
/// </summary>
[CollectionDefinition("RegionManagerCollection")]
public class RegionManagerCollection { }
