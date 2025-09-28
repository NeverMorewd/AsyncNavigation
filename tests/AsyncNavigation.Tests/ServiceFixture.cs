using Microsoft.Extensions.DependencyInjection;

namespace AsyncNavigation.Tests;

public class ServiceFixture : IDisposable
{
    public IServiceProvider ServiceProvider { get; }

    public ServiceFixture()
    {
        ServiceCollection serviceDescriptors = new();
        serviceDescriptors.RegisterNavigationFramework();
        ServiceProvider = serviceDescriptors.BuildServiceProvider();
    }

    public void Dispose()
    {
        if (ServiceProvider is IDisposable d) d.Dispose();
    }
}
