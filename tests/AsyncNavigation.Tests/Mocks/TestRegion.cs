using AsyncNavigation.Abstractions;

namespace AsyncNavigation.Tests.Mocks;

public class TestRegion : RegionBase<TestRegion, object>, IRegionPresenter
{
    
    public TestRegion(string name, object control, IServiceProvider serviceProvider) : base(name, control, serviceProvider)
    {

    }
    public bool IsActive { get; private set; }
    public override void ProcessActivate(NavigationContext navigationContext)
    {
        IsActive = true;
    }

    public override void ProcessDeactivate(NavigationContext navigationContext)
    {
        IsActive = false;
    }

    public override void RenderIndicator(NavigationContext navigationContext)
    {
        IsActive = true;
    }

    public static TestRegion Build(IServiceProvider serviceProvider)
    {
        return new TestRegion("TestRegion", new object(), serviceProvider);
    }
}
