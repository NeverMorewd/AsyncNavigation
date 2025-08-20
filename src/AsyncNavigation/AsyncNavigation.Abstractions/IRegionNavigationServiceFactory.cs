namespace AsyncNavigation.Abstractions;

internal interface IRegionNavigationServiceFactory
{
    IRegionNavigationService<T> Create<T>(T region) where T : IRegionPresenter;
}
