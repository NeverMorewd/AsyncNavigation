using AsyncNavigation.Abstractions;
using AsyncNavigation.Core;

namespace AsyncNavigation.Tests.Mocks;

public class TestRegion : RegionBase<TestRegion, object>, IRegionPresenter
{
    
    public TestRegion(string name, object control, IServiceProvider serviceProvider) : base(name, control, serviceProvider)
    {

    }
    public static TestRegion GetOne(IServiceProvider serviceProvider)
    {
        return new TestRegion("TestRegion", new object(), serviceProvider);
    }
    public bool IsActive { get; private set; }
    public NavigationContext? Current { get; private set; }

    public override NavigationPipelineMode NavigationPipelineMode => NavigationPipelineMode.RenderFirst;

    public override void ProcessActivate(NavigationContext navigationContext)
    {
        IsActive = true;
        Current = navigationContext;
    }

    public override void ProcessDeactivate(NavigationContext? navigationContext)
    {
        IsActive = false;
        Current = null;
    }

    
    public static TestRegion Build(IServiceProvider serviceProvider)
    {
        return new TestRegion("TestRegion", new object(), serviceProvider);
    }
}
