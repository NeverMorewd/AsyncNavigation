using AsyncNavigation.Abstractions;

namespace AsyncNavigation;

public abstract class RegionAdapterBase<T> : IRegionAdapter<T>
{
    public virtual uint Priority => 0;

    public virtual bool IsAdapted(T control)
    {
        if (control == null) return false;
        return control.GetType() == typeof(T);
    }
    public abstract IRegion CreateRegion(string name, T control, IServiceProvider serviceProvider, bool? useCache);

    bool IRegionAdapter.IsAdapted(object control) =>
        control is T t && IsAdapted(t);

    IRegion IRegionAdapter.CreateRegion(string name, object control, IServiceProvider serviceProvider, bool? useCache)
    {
        if (control is not T t)
            throw new ArgumentException($"Control type {control.GetType()} is not supported.");
        return CreateRegion(name, t, serviceProvider, useCache);
    }
}
