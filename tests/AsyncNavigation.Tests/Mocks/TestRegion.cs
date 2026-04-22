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

    public override Task ProcessActivateAsync(NavigationContext navigationContext)
    {
        IsActive = true;
        Current = navigationContext;
        return Task.CompletedTask;
    }

    public override Task ProcessDeactivateAsync(NavigationContext? navigationContext)
    {
        IsActive = false;
        Current = null;
        return Task.CompletedTask;
    }

    
    public static TestRegion Build(IServiceProvider serviceProvider)
    {
        return new TestRegion("TestRegion", new object(), serviceProvider);
    }
}
