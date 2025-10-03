using AsyncNavigation.Abstractions;

namespace AsyncNavigation.Tests.Mocks;

public class TestView : IView
{
    public object? DataContext { get; set; }
}
