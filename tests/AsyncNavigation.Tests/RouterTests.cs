using AsyncNavigation.Core;

namespace AsyncNavigation.Tests;

public class RouterTests
{
    // Every MapNavigation call requires at least one NavigationTarget
    private static readonly NavigationTarget DefaultTarget = new("Main", "Home");

    // -----------------------------------------------------------------------
    // Registration
    // -----------------------------------------------------------------------

    [Fact]
    public void MapNavigation_EmptyPath_ThrowsArgumentException()
    {
        var router = new Router();
        Assert.Throws<ArgumentException>(() => router.MapNavigation("", DefaultTarget));
        Assert.Throws<ArgumentException>(() => router.MapNavigation("   ", DefaultTarget));
    }

    [Fact]
    public void MapNavigation_NoTargets_ThrowsArgumentException()
    {
        var router = new Router();
        Assert.Throws<ArgumentException>(() => router.MapNavigation("/home"));
    }

    [Fact]
    public void MapNavigation_DuplicatePath_ThrowsInvalidOperationException()
    {
        var router = new Router();
        router.MapNavigation("/home", DefaultTarget);
        Assert.Throws<InvalidOperationException>(() => router.MapNavigation("/home", DefaultTarget));
    }

    [Fact]
    public void Routes_ReturnsAllRegisteredRoutes()
    {
        var router = new Router();
        router.MapNavigation("/a", DefaultTarget);
        router.MapNavigation("/b", DefaultTarget);
        router.MapNavigation("/c", DefaultTarget);
        Assert.Equal(3, router.Routes.Count);
    }

    // -----------------------------------------------------------------------
    // Exact match
    // -----------------------------------------------------------------------

    [Fact]
    public void Match_ExactPath_ReturnsRoute()
    {
        var router = new Router();
        router.MapNavigation("/home", DefaultTarget);
        var route = router.Match("/home");
        Assert.NotNull(route);
        Assert.Equal("/home", route.Path);
    }

    [Fact]
    public void Match_ExactPath_IsCaseInsensitive()
    {
        var router = new Router();
        router.MapNavigation("/Home", DefaultTarget);
        Assert.NotNull(router.Match("/home"));
        Assert.NotNull(router.Match("/HOME"));
    }

    [Fact]
    public void Match_UnknownPath_ReturnsNull()
    {
        var router = new Router();
        router.MapNavigation("/home", DefaultTarget);
        Assert.Null(router.Match("/unknown"));
    }

    // -----------------------------------------------------------------------
    // Segment matching
    // -----------------------------------------------------------------------

    [Fact]
    public void Match_MultiSegmentPath_ReturnsCorrectRoute()
    {
        var router = new Router();
        router.MapNavigation("/settings/profile", DefaultTarget);
        router.MapNavigation("/settings/account", DefaultTarget);
        var route = router.Match("/settings/profile");
        Assert.NotNull(route);
        Assert.Equal("/settings/profile", route.Path);
    }

    [Fact]
    public void Match_LongerSegmentRoutePreferredOverShorter()
    {
        var router = new Router();
        router.MapNavigation("/a", DefaultTarget);
        router.MapNavigation("/a/b/c", DefaultTarget);
        router.MapNavigation("/a/b", DefaultTarget);
        var route = router.Match("/a/b/c");
        Assert.NotNull(route);
        Assert.Equal("/a/b/c", route.Path);
    }

    [Fact]
    public void Match_LeadingSlashIsOptional()
    {
        var router = new Router();
        router.MapNavigation("/home", DefaultTarget);
        Assert.NotNull(router.Match("/home"));
        Assert.NotNull(router.Match("home"));
    }

    [Fact]
    public void Match_NullPath_ReturnsNull()
    {
        var router = new Router();
        router.MapNavigation("/home", DefaultTarget);
        // null treated as empty string – no route matches
        var result = router.Match(null!);
        Assert.Null(result);
    }

    // -----------------------------------------------------------------------
    // Fallback
    // -----------------------------------------------------------------------

    [Fact]
    public void WithFallback_StoresFallbackOnRoute()
    {
        var router = new Router();
        var fallbackTarget = new NavigationTarget("Main", "Error");
        router.MapNavigation("/home", DefaultTarget).WithFallback(fallbackTarget);
        var route = router.Match("/home");
        Assert.NotNull(route);
        Assert.NotNull(route.Fallback);
        Assert.Equal("Error", route.Fallback!.ViewName);
    }

    // -----------------------------------------------------------------------
    // Cache invalidation
    // -----------------------------------------------------------------------

    [Fact]
    public void Match_SortedCacheInvalidatedAfterAdd()
    {
        var router = new Router();
        router.MapNavigation("/a/b", DefaultTarget);
        _ = router.Match("/a/b");               // trigger cache build
        router.MapNavigation("/a/b/c", DefaultTarget);  // invalidate cache
        var route = router.Match("/a/b/c");
        Assert.NotNull(route);
        Assert.Equal("/a/b/c", route.Path);
    }

    // -----------------------------------------------------------------------
    // Route targets
    // -----------------------------------------------------------------------

    [Fact]
    public void Route_ContainsExpectedTargets()
    {
        var router = new Router();
        var target = new NavigationTarget("ContentRegion", "Dashboard");
        router.MapNavigation("/dashboard", target);
        var route = router.Match("/dashboard");
        Assert.NotNull(route);
        Assert.Single(route.Targets);
        Assert.Equal("Dashboard", route.Targets[0].ViewName);
        Assert.Equal("ContentRegion", route.Targets[0].RegionName);
    }
}
