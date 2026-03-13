using AsyncNavigation.Core;

namespace AsyncNavigation.Tests;

public class RegionNavigationHistoryTests
{
    private static NavigationContext MakeContext(string view = "View") =>
        new() { RegionName = "Main", ViewName = view };

    private static RegionNavigationHistory MakeHistory(int max = 10) =>
        new(new NavigationOptions { MaxHistoryItems = max });

    // -----------------------------------------------------------------------
    // Initial state
    // -----------------------------------------------------------------------

    [Fact]
    public void InitialState_IsEmpty()
    {
        var h = MakeHistory();

        Assert.False(h.CanGoBack);
        Assert.False(h.CanGoForward);
        Assert.Null(h.Current);
        Assert.Empty(h.History);
    }

    // -----------------------------------------------------------------------
    // Add
    // -----------------------------------------------------------------------

    [Fact]
    public void Add_NullContext_ThrowsArgumentNullException()
    {
        var h = MakeHistory();
        Assert.Throws<ArgumentNullException>(() => h.Add(null!));
    }

    [Fact]
    public void Add_SingleItem_CurrentIsSet()
    {
        var h = MakeHistory();
        var ctx = MakeContext("A");
        h.Add(ctx);

        Assert.Same(ctx, h.Current);
        Assert.False(h.CanGoBack);
        Assert.False(h.CanGoForward);
    }

    [Fact]
    public void Add_TwoItems_CanGoBack()
    {
        var h = MakeHistory();
        h.Add(MakeContext("A"));
        h.Add(MakeContext("B"));

        Assert.True(h.CanGoBack);
        Assert.False(h.CanGoForward);
    }

    // -----------------------------------------------------------------------
    // GoBack / GoForward
    // -----------------------------------------------------------------------

    [Fact]
    public void GoBack_WhenEmpty_ReturnsNull()
    {
        var h = MakeHistory();
        Assert.Null(h.GoBack());
    }

    [Fact]
    public void GoForward_WhenAtEnd_ReturnsNull()
    {
        var h = MakeHistory();
        h.Add(MakeContext("A"));
        Assert.Null(h.GoForward());
    }

    [Fact]
    public void GoBack_MovesIndexBack()
    {
        var h = MakeHistory();
        var a = MakeContext("A");
        var b = MakeContext("B");
        h.Add(a);
        h.Add(b);

        var result = h.GoBack();

        Assert.Same(a, result);
        Assert.Same(a, h.Current);
        Assert.True(h.CanGoForward);
        Assert.False(h.CanGoBack);
    }

    [Fact]
    public void GoForward_MovesIndexForward()
    {
        var h = MakeHistory();
        var a = MakeContext("A");
        var b = MakeContext("B");
        h.Add(a);
        h.Add(b);
        h.GoBack();

        var result = h.GoForward();

        Assert.Same(b, result);
        Assert.Same(b, h.Current);
    }

    [Fact]
    public void RoundTrip_BackAndForward_RestoresSameContext()
    {
        var h = MakeHistory();
        var ctxs = Enumerable.Range(0, 5)
                             .Select(i => MakeContext($"View{i}"))
                             .ToList();
        ctxs.ForEach(h.Add);

        // Walk all the way back
        while (h.CanGoBack) h.GoBack();

        Assert.Same(ctxs[0], h.Current);

        // Walk all the way forward
        while (h.CanGoForward) h.GoForward();

        Assert.Same(ctxs[4], h.Current);
    }

    // -----------------------------------------------------------------------
    // Branch truncation: adding after GoBack removes forward entries
    // -----------------------------------------------------------------------

    [Fact]
    public void Add_AfterGoBack_TruncatesForwardHistory()
    {
        var h = MakeHistory();
        h.Add(MakeContext("A"));
        h.Add(MakeContext("B"));
        h.Add(MakeContext("C"));
        h.GoBack(); // now at B
        h.GoBack(); // now at A

        var d = MakeContext("D");
        h.Add(d);

        Assert.False(h.CanGoForward);
        Assert.Equal(2, h.History.Count);  // A and D remain
        Assert.Same(d, h.Current);
    }

    // -----------------------------------------------------------------------
    // MaxHistoryItems
    // -----------------------------------------------------------------------

    [Fact]
    public void Add_ExceedMaxHistory_OldestIsDropped()
    {
        var h = MakeHistory(max: 3);
        var a = MakeContext("A");
        var b = MakeContext("B");
        var c = MakeContext("C");
        var d = MakeContext("D");

        h.Add(a);
        h.Add(b);
        h.Add(c);
        h.Add(d);   // A should be evicted

        Assert.Equal(3, h.History.Count);
        Assert.DoesNotContain(a, h.History);
        Assert.Same(d, h.Current);
        Assert.True(h.CanGoBack);
    }

    [Fact]
    public void Add_MaxHistory_CanStillGoBack()
    {
        var h = MakeHistory(max: 2);
        h.Add(MakeContext("A"));
        h.Add(MakeContext("B"));
        h.Add(MakeContext("C")); // evicts A

        Assert.True(h.CanGoBack);

        var prev = h.GoBack();
        Assert.Equal("B", prev?.ViewName);
    }

    // -----------------------------------------------------------------------
    // Clear
    // -----------------------------------------------------------------------

    [Fact]
    public void Clear_ResetsEverything()
    {
        var h = MakeHistory();
        h.Add(MakeContext("A"));
        h.Add(MakeContext("B"));
        h.GoBack();

        h.Clear();

        Assert.Empty(h.History);
        Assert.Null(h.Current);
        Assert.False(h.CanGoBack);
        Assert.False(h.CanGoForward);
    }
}
