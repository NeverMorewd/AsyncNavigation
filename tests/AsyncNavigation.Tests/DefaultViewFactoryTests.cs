using AsyncNavigation.Abstractions;
using AsyncNavigation.Tests.Infrastructure;
using AsyncNavigation.Tests.Mocks;
using Microsoft.Extensions.DependencyInjection;

namespace AsyncNavigation.Tests;

public class DefaultViewFactoryTests : IClassFixture<ServiceFixture>
{
    private readonly IServiceProvider _serviceProvider;

    public DefaultViewFactoryTests(ServiceFixture serviceFixture)
    {
        _serviceProvider = serviceFixture.ServiceProvider;
    }

    [Fact]
    public void CreateView_ShouldReturnView_WithNavigationAwareDataContext()
    {
        var factory = _serviceProvider.GetRequiredService<IViewFactory>();
        var view = factory.CreateView("TestView") as TestView;
        Assert.NotNull(view);
        Assert.NotNull(view.DataContext);
        Assert.IsType<TestNavigationAware>(view.DataContext);
    }

    [Fact]
    public void CanCreateView_ShouldReturnTrueForRegisteredView()
    {
        var factory = _serviceProvider.GetRequiredService<IViewFactory>();
        Assert.True(factory.CanCreateView("TestView"));
        Assert.False(factory.CanCreateView("NonExistentView"));
    }

    [Fact]
    public void AddView_Instance_ShouldRetrieveSameInstance()
    {
        var factory = _serviceProvider.GetRequiredService<IViewFactory>();
        var instance = new TestView();

        factory.AddView("InstanceView", instance);

        var retrieved = factory.CreateView("InstanceView");
        Assert.Same(instance, retrieved);
    }

    [Fact]
    public void AddView_Factory_ShouldCreateNewInstances()
    {
        var factory = _serviceProvider.GetRequiredService<IViewFactory>();

        factory.AddView("FactoryView", key => new TestView());

        var first = factory.CreateView("FactoryView");
        var second = factory.CreateView("FactoryView");

        Assert.NotSame(first, second);
        Assert.IsType<TestView>(first);
        Assert.IsType<TestView>(second);
    }

    [Fact]
    public void AddView_DuplicateKey_ShouldThrow()
    {
        var factory = _serviceProvider.GetRequiredService<IViewFactory>();
        factory.AddView("DuplicateView", new TestView());

        Assert.Throws<ArgumentException>(() =>
            factory.AddView("DuplicateView", new TestView()));
    }
}
