namespace AsyncNavigation.Abstractions;

public interface IRegionIndicatorHost<out TControl> : IRegionIndicator
{
    new TControl IndicatorControl { get; }
}
