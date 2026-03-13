using AsyncNavigation.Abstractions;

namespace AsyncNavigation.Tests.Mocks;

public class TestViewAwareView : IView
{
    public object? DataContext { get; set; }
}
