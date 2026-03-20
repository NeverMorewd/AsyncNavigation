using AsyncNavigation.Abstractions;
using AsyncNavigation.Tests.Mocks;
using Microsoft.Extensions.DependencyInjection;

namespace AsyncNavigation.Tests.Infrastructure;

public class InterceptorServiceFixture : IDisposable
{
    public IServiceProvider ServiceProvider { get; }
    public TrackingInterceptor Interceptor { get; }

    public InterceptorServiceFixture()
    {
        Interceptor = new TrackingInterceptor();

        ServiceCollection services = new();
        services.AddNavigationTestSupport();
        services.RegisterView<TestView, TestNavigationAware>("TestView");
        services.RegisterView<AnotherTestView, TestNavigationAware>("AnotherTestView");
        // Register interceptor before the service provider is built
        services.AddSingleton<INavigationInterceptor>(Interceptor);

        ServiceProvider = services.BuildServiceProvider();
    }

    public void Dispose()
    {
        if (ServiceProvider is IDisposable d) d.Dispose();
    }
}
