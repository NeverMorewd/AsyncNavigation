using AsyncNavigation.Abstractions;
using AsyncNavigation.Tests.Mocks;
using Microsoft.Extensions.DependencyInjection;

namespace AsyncNavigation.Tests.Infrastructure;

public class ServiceFixture : IDisposable
{
    public IServiceProvider ServiceProvider { get; }

    public ServiceFixture()
    {
        ServiceCollection serviceDescriptors = new();
        serviceDescriptors.RegisterNavigationFramework();
        serviceDescriptors.AddTransient<IInnerRegionIndicatorHost, TestInnerIndicatorHost>();
        serviceDescriptors.RegisterView<TestView, TestNavigationAware>("TestView");
        serviceDescriptors.RegisterView<AnotherTestView, TestNavigationAware>("AnotherTestView");
        ServiceProvider = serviceDescriptors.BuildServiceProvider();
    }

    public void Dispose()
    {
        if (ServiceProvider is IDisposable d) d.Dispose();
    }
}
