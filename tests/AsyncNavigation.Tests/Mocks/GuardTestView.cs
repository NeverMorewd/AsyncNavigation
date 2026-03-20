using AsyncNavigation.Abstractions;

namespace AsyncNavigation.Tests.Mocks;

public class GuardTestView : IView
{
    public GuardTestView(GuardTestNavigationAware vm) => DataContext = vm;
    public object? DataContext { get; set; }
}
