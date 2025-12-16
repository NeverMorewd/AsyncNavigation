using AsyncNavigation.Abstractions;

namespace AsyncNavigation.Core;

public sealed class RegionChangeEventArgs(IRegion region, RegionChangeKind changeKind) : EventArgs
{
    public IRegion Region { get; } = region;
    public RegionChangeKind RegionChangeKind { get; } = changeKind;
}