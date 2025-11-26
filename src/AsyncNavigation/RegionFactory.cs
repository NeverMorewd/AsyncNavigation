using System.Collections.Immutable;
using AsyncNavigation.Abstractions;

namespace AsyncNavigation;

internal sealed class RegionFactory : IRegionFactory
{
    private ImmutableArray<IRegionAdapter> _adapters = ImmutableArray<IRegionAdapter>.Empty;

    public RegionFactory(IEnumerable<IRegionAdapter> adapters)
    {
        if (adapters != null && adapters.Any())
        {
            _adapters = [.. adapters];
        }
        else
        {
            _adapters = [];
        }
    }

    /// <summary>
    /// Registers a region adapter for use in the application.
    /// </summary>
    /// <param name="adapter">The region adapter to register. Cannot be <see langword="null"/>.</param>
    public void RegisterAdapter(IRegionAdapter adapter)
    {
        ArgumentNullException.ThrowIfNull(adapter);
        _adapters = _adapters.Add(adapter);
    }

    private IRegionAdapter? GetAdapter(object control)
    {
        var snapshot = _adapters;

        return snapshot
            .OrderByDescending(a => a.Priority)
            .ThenBy(a => a.GetType().FullName)
            .FirstOrDefault(a => a.IsAdapted(control));
    }

    public IRegion CreateRegion(
        string name,
        object control,
        IServiceProvider serviceProvider,
        bool? useCache = null)
    {
        ArgumentNullException.ThrowIfNull(control);

        var adapter = GetAdapter(control)
            ?? throw new NotSupportedException(
                $"No adapter found for control type: {control.GetType().Name}");

        return adapter.CreateRegion(name, control, serviceProvider, useCache);
    }
}
