namespace AsyncNavigation.Abstractions;

public interface IRegionNavigationServiceFactory
{
    IRegionNavigationService<T> Create<T>(T region) where T : IRegionPresenter;
}
