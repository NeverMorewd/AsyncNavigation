using AsyncNavigation.Core;
using Avalonia.Controls;

namespace AsyncNavigation.Avalonia;

internal class NavigationPageRegion : RegionBase<NavigationPageRegion, NavigationPage>
{
    private readonly NavigationPage _navigationPage;
    public NavigationPageRegion(string name,
        NavigationPage navigationPage,
        IServiceProvider serviceProvider,
        bool? useCache) : base(name, navigationPage, serviceProvider)
    {
        _navigationPage = navigationPage;
    }
    public override NavigationPipelineMode NavigationPipelineMode => NavigationPipelineMode.RenderFirst;

    protected override void InitializeOnRegionCreated(NavigationPage control)
    {
        base.InitializeOnRegionCreated(control);
    }
    public override void ProcessActivate(NavigationContext navigationContext)
    {
        _navigationPage.PushAsync(new ContentPage { Content = navigationContext?.IndicatorHost.Value?.Host as Control });
    }

    public override void ProcessDeactivate(NavigationContext? navigationContext)
    {
        _navigationPage.PushAsync(new ContentPage { Content = null });
    }
}
