namespace AsyncNavigation.Abstractions;

public abstract class RegionAdapterBase<T> : IRegionAdapter<T>
{
    /// <summary>
    /// TODO:
    /// </summary>
    /// <param name="control"></param>
    /// <returns></returns>
    public virtual bool IsAdapted(T control)
    {
        return true;
    }
    public abstract IRegion CreateRegion(string name, T control, IServiceProvider serviceProvider);

    bool IRegionAdapter.IsAdapted(object control) =>
        control is T t && IsAdapted(t);

    IRegion IRegionAdapter.CreateRegion(string name, object control, IServiceProvider serviceProvider)
    {
        if (control is not T t)
            throw new ArgumentException($"Control type {control.GetType()} is not supported.");
        return CreateRegion(name, t, serviceProvider);
    }
}
