using AsyncNavigation.Tests.Mocks;
using Microsoft.Extensions.DependencyInjection;

namespace AsyncNavigation.Tests.Infrastructure;

public class ServiceFixture : IDisposable
{
    public IServiceProvider ServiceProvider { get; }

    public ServiceFixture()
    {
        ServiceCollection serviceDescriptors = new();

        NavigationOptions navigationOptions = new()
        {
            MaxHistoryItems = 10
        };
        serviceDescriptors.AddNavigationTestSupport(navigationOptions);

        serviceDescriptors.RegisterView<TestView, TestNavigationAware>("TestView");
        serviceDescriptors.RegisterView<AnotherTestView, TestNavigationAware>("AnotherTestView");
        serviceDescriptors.RegisterView<TestViewAwareView, TestViewAwareNavigationAware>("ViewAwareView");
        
        ServiceProvider = serviceDescriptors.BuildServiceProvider();
    }

    public void Dispose()
    {
        if (ServiceProvider is IDisposable d) d.Dispose();
    }
}
