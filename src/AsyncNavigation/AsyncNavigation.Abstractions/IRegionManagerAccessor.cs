namespace AsyncNavigation.Abstractions;

public interface IRegionManagerAccessor
{
    IRegionManager? Current { get; }
    void SetCurrent(IRegionManager manager);
}
