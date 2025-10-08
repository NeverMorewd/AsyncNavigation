using AsyncNavigation;
using AsyncNavigation.Abstractions;
using AsyncNavigation.Tests.Mocks;

namespace Microsoft.Extensions.DependencyInjection;

public static class Extensions
{
    public static IServiceCollection AddNavigationTestSupport(this IServiceCollection serviceDescriptors, NavigationOptions? navigationOptions = null)
    {
        return serviceDescriptors
            .RegisterNavigationFramework(navigationOptions)
            .AddTransient<IInnerRegionIndicatorHost, TestInnerIndicatorHost>()
            .AddSingleton<IRegionManager, RegionManager>();
    }
}
