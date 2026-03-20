using AsyncNavigation.Abstractions;
using AsyncNavigation.Tests.Mocks;
using Microsoft.Extensions.DependencyInjection;

namespace AsyncNavigation.Tests.Infrastructure;

public class ServiceFixture : IDisposable
{
    public IServiceProvider ServiceProvider { get; }
    public TrackingInterceptor Interceptor { get; }

    public ServiceFixture()
    {
        Interceptor = new TrackingInterceptor();

        ServiceCollection serviceDescriptors = new();

        NavigationOptions navigationOptions = new()
        {
            MaxHistoryItems = 10
        };
        serviceDescriptors.AddNavigationTestSupport(navigationOptions);

        serviceDescriptors.RegisterView<TestView, TestNavigationAware>("TestView");
        serviceDescriptors.RegisterView<AnotherTestView, TestNavigationAware>("AnotherTestView");
        serviceDescriptors.RegisterView<GuardTestView, GuardTestNavigationAware>("GuardTestView");
        serviceDescriptors.AddSingleton<INavigationInterceptor>(Interceptor);

        ServiceProvider = serviceDescriptors.BuildServiceProvider();
    }

    public void Dispose()
    {
        if (ServiceProvider is IDisposable d) d.Dispose();
    }
}
