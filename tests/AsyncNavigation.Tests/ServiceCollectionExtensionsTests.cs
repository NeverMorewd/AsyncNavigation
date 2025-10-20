using AsyncNavigation.Abstractions;
using AsyncNavigation.Tests.Mocks;
using Microsoft.Extensions.DependencyInjection;

namespace AsyncNavigation.Tests;

public class ServiceCollectionExtensionsTests
{
    private readonly ServiceCollection _services;

    public ServiceCollectionExtensionsTests()
    {
        _services = new ServiceCollection();
    }

    #region RegisterView Tests

    [Fact]
    public void RegisterView_ValidParameters_RegistersServicesCorrectly()
    {
        // Arrange
        var viewKey = "testView";

        // Act
        _services.RegisterView<TestView, TestNavigationAware>(viewKey);
        var serviceProvider = _services.BuildServiceProvider();

        // Assert
        var view = serviceProvider.GetRequiredKeyedService<TestView>(viewKey);
        var viewModel = serviceProvider.GetRequiredKeyedService<TestNavigationAware>(viewKey);
        var keyedView = serviceProvider.GetRequiredKeyedService<IView>(viewKey);

        Assert.NotNull(view);
        Assert.NotNull(viewModel);
        Assert.NotNull(keyedView);
        Assert.IsType<TestView>(keyedView);
    }

    [Fact]
    public void RegisterView_NullServices_ThrowsArgumentNullException()
    {
        // Arrange
        IServiceCollection services = null!;
        var viewKey = "testView";

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            services.RegisterView<TestView, TestNavigationAware>(viewKey));
    }

    [Fact]
    public void RegisterView_NullViewKey_ThrowsArgumentNullException()
    {
        // Arrange
        object viewKey = null!;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            _services.RegisterView<TestView, TestNavigationAware>(viewKey));
    }

    [Fact]
    public void RegisterView_ViewModelWithoutRequiredInterface_ThrowsInvalidOperationException()
    {
        // Arrange
        var viewKey = "invalidView";

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() =>
            _services.RegisterView<TestView, TestView>(viewKey));

        Assert.Contains("must implement at least one of the following interfaces", exception.Message);
    }

    [Fact]
    public void RegisterView_DataContextIsSetCorrectly()
    {
        // Arrange
        var viewKey = "dataContextView";

        // Act
        _services.RegisterView<TestView, TestNavigationAware>(viewKey);
        var serviceProvider = _services.BuildServiceProvider();

        // Assert
        var keyedView = serviceProvider.GetRequiredKeyedService<IView>(viewKey) as TestView;
        Assert.NotNull(keyedView);
        Assert.NotNull(keyedView.DataContext);
        Assert.IsType<TestNavigationAware>(keyedView.DataContext);
    }

    [Fact]
    public void RegisterView_SameViewDifferentViewModels_RegistersCorrectly()
    {
        // Arrange
        var viewKey1 = "view1";
        var viewKey2 = "view2";

        // Act
        _services.RegisterView<TestView, TestNavigationAware>(viewKey1);
        _services.RegisterView<TestView, AnotherTestNavigationAware>(viewKey2);

        var serviceProvider = _services.BuildServiceProvider();

        // Assert
        var view1 = serviceProvider.GetRequiredKeyedService<IView>(viewKey1);
        var view2 = serviceProvider.GetRequiredKeyedService<IView>(viewKey2);

        Assert.NotNull(view1);
        Assert.NotNull(view2);
        Assert.IsType<TestView>(view1);
        Assert.IsType<TestView>(view2);
        Assert.IsType<TestNavigationAware>(view1.DataContext);
        Assert.IsType<AnotherTestNavigationAware>(view2.DataContext);
    }

    [Fact]
    public void RegisterView_MultiAwareViewModel_RegistersAllInterfaces()
    {
        // Arrange
        var viewKey = "multiAwareView";

        // Act
        _services.RegisterView<TestView, TestMultiAwareViewModel>(viewKey);
        var serviceProvider = _services.BuildServiceProvider();

        // Assert
        var view = serviceProvider.GetRequiredKeyedService<IView>(viewKey);
        var navigationAware = serviceProvider.GetRequiredKeyedService<INavigationAware>(viewKey);
        var dialogAware = serviceProvider.GetRequiredKeyedService<IDialogAware>(viewKey);
        
        Assert.NotNull(view);
        Assert.NotNull(navigationAware);
        Assert.NotNull(dialogAware);

        Assert.IsType<TestMultiAwareViewModel>(navigationAware);
        Assert.IsType<TestMultiAwareViewModel>(dialogAware);
        Assert.IsType<TestMultiAwareViewModel>(view.DataContext);
    }

    #endregion

    #region RegisterNavigation Tests

    [Fact]
    public void RegisterNavigation_ValidParameters_RegistersServicesCorrectly()
    {
        // Arrange
        var viewKey = "navigationView";

        // Act
        _services.RegisterNavigation<TestView, TestNavigationAware>(viewKey);
        var serviceProvider = _services.BuildServiceProvider();

        // Assert
        var view = serviceProvider.GetRequiredKeyedService<TestView>(viewKey);
        var viewModel = serviceProvider.GetRequiredKeyedService<TestNavigationAware>(viewKey);
        var keyedView = serviceProvider.GetRequiredKeyedService<IView>(viewKey);

        Assert.NotNull(view);
        Assert.NotNull(viewModel);
        Assert.NotNull(keyedView.DataContext);
        Assert.IsAssignableFrom<INavigationAware>(keyedView.DataContext);
        Assert.IsNotAssignableFrom<IDialogAware>(keyedView.DataContext);
    }

    [Fact]
    public void RegisterNavigation_NullServices_ThrowsArgumentNullException()
    {
        // Arrange
        IServiceCollection services = null!;
        var viewKey = "navigationView";

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            services.RegisterNavigation<TestView, TestNavigationAware>(viewKey));
    }

    [Fact]
    public void RegisterNavigation_WithViewModelBuilder_RegistersCorrectly()
    {
        // Arrange
        var viewKey = "customNavigationView";
        var customViewModel = new TestNavigationAware();
        TestNavigationAware viewModelBuilder(IServiceProvider sp, object? key) => customViewModel;

        // Act
        _services.RegisterNavigation<TestView, TestNavigationAware>(viewKey, viewModelBuilder);
        var serviceProvider = _services.BuildServiceProvider();

        // Assert
        var keyedViewModel = serviceProvider.GetRequiredKeyedService<TestNavigationAware>(viewKey);
        var keyedView = serviceProvider.GetRequiredKeyedService<IView>(viewKey);

        Assert.Same(customViewModel, keyedViewModel);
        Assert.NotNull(keyedView);
        Assert.IsType<TestNavigationAware>(keyedView.DataContext);
    }

    [Fact]
    public void RegisterNavigation_WithViewModelBuilder_NullBuilder_ThrowsArgumentNullException()
    {
        // Arrange
        var viewKey = "navigationView";
        Func<IServiceProvider, object?, TestNavigationAware> viewModelBuilder = null!;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            _services.RegisterNavigation<TestView, TestNavigationAware>(viewKey, viewModelBuilder));
    }

    [Fact]
    public void RegisterNavigation_DuplicateKey_DifferentViewTypes_RegistersBoth()
    {
        // Arrange
        var sameKey = "sameKey";

        // Act
        _services.RegisterNavigation<TestView, TestNavigationAware>(sameKey);
        _services.RegisterNavigation<AnotherTestView, AnotherTestNavigationAware>(sameKey);

        var serviceProvider = _services.BuildServiceProvider();

        // Assert
        var keyedView = serviceProvider.GetRequiredKeyedService<IView>(sameKey);
        Assert.NotNull(keyedView);
        Assert.IsType<AnotherTestView>(keyedView);
        Assert.IsType<AnotherTestNavigationAware>(keyedView.DataContext);
    }

    [Fact]
    public void RegisterNavigation_DataContextIsSetFromCustomBuilder()
    {
        // Arrange
        var viewKey = "customDataContextView";
        var customViewModel = new TestNavigationAware();
        TestNavigationAware viewModelBuilder(IServiceProvider sp, object? key) => customViewModel;

        // Act
        _services.RegisterNavigation<TestView, TestNavigationAware>(viewKey, viewModelBuilder);
        var serviceProvider = _services.BuildServiceProvider();

        // Assert
        var keyedView = serviceProvider.GetRequiredKeyedService<IView>(viewKey) as TestView;
        Assert.NotNull(keyedView);
        Assert.Same(customViewModel, keyedView.DataContext);
    }

    #endregion

    #region RegisterDialog Tests

    [Fact]
    public void RegisterDialog_ValidParameters_RegistersServicesCorrectly()
    {
        // Arrange
        var viewKey = "dialogView";

        // Act
        _services.RegisterDialog<TestView, TestDialogAware>(viewKey);
        var serviceProvider = _services.BuildServiceProvider();

        // Assert
        var view = serviceProvider.GetRequiredKeyedService<TestView>(viewKey);
        var viewModel = serviceProvider.GetRequiredKeyedService<TestDialogAware>(viewKey);
        var keyedView = serviceProvider.GetRequiredKeyedService<IView>(viewKey);

        Assert.NotNull(view);
        Assert.NotNull(viewModel);
        Assert.NotNull(keyedView);
        Assert.IsType<TestDialogAware>(keyedView.DataContext);
    }

    [Fact]
    public void RegisterDialog_NullServices_ThrowsArgumentNullException()
    {
        // Arrange
        IServiceCollection services = null!;
        var viewKey = "dialogView";

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            services.RegisterDialog<TestView, TestDialogAware>(viewKey));
    }

    [Fact]
    public void RegisterDialog_WithViewModelBuilder_RegistersCorrectly()
    {
        // Arrange
        var viewKey = "customDialogView";
        var customViewModel = new TestDialogAware();
        TestDialogAware viewModelBuilder(IServiceProvider sp, object? key) => customViewModel;

        // Act
        _services.RegisterDialog<TestView, TestDialogAware>(viewKey, viewModelBuilder);
        var serviceProvider = _services.BuildServiceProvider();

        // Assert
        var keyedViewModel = serviceProvider.GetRequiredKeyedService<TestDialogAware>(viewKey);
        var keyedView = serviceProvider.GetRequiredKeyedService<IView>(viewKey);

        Assert.Same(customViewModel, keyedViewModel);
        Assert.NotNull(keyedView);
        Assert.Same(customViewModel, keyedView.DataContext);
    }

    [Fact]
    public void RegisterDialog_WithViewModelBuilder_NullBuilder_ThrowsArgumentNullException()
    {
        // Arrange
        var viewKey = "dialogView";
        Func<IServiceProvider, object?, TestDialogAware> viewModelBuilder = null!;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            _services.RegisterDialog<TestView, TestDialogAware>(viewKey, viewModelBuilder));
    }

    [Fact]
    public void RegisterDialog_MixedRegistrations_SameKey_DifferentTypes()
    {
        // Arrange
        var mixedKey = "mixedKey";

        // Act
        _services.RegisterView<TestView, TestMultiAwareViewModel>(mixedKey);
        _services.RegisterNavigation<AnotherTestView, AnotherTestNavigationAware>(mixedKey);
        _services.RegisterDialog<TestView, TestDialogAware>(mixedKey);

        var serviceProvider = _services.BuildServiceProvider();

        // Assert
        var keyedView = serviceProvider.GetRequiredKeyedService<IView>(mixedKey);
        Assert.NotNull(keyedView);
        Assert.IsType<TestView>(keyedView);
        Assert.IsType<TestDialogAware>(keyedView.DataContext);
    }

    [Fact]
    public void RegisterDialog_DataContextIsSetFromCustomBuilder()
    {
        // Arrange
        var viewKey = "dialogCustomDataContext";
        var customViewModel = new TestDialogAware();
        TestDialogAware viewModelBuilder(IServiceProvider sp, object? key) => customViewModel;

        // Act
        _services.RegisterDialog<TestView, TestDialogAware>(viewKey, viewModelBuilder);
        var serviceProvider = _services.BuildServiceProvider();

        // Assert
        var keyedView = serviceProvider.GetRequiredKeyedService<IView>(viewKey) as TestView;
        Assert.NotNull(keyedView);
        Assert.Same(customViewModel, keyedView.DataContext);
    }

    #endregion

    #region Edge Cases Tests

    [Fact]
    public void DifferentKeys_SameViewAndViewModel_RegistersMultipleInstances()
    {
        // Arrange
        var key1 = "key1";
        var key2 = "key2";

        // Act
        _services.RegisterView<TestView, TestNavigationAware>(key1);
        _services.RegisterView<TestView, AnotherTestNavigationAware>(key2);

        var serviceProvider = _services.BuildServiceProvider();

        // Assert
        var view1 = serviceProvider.GetRequiredKeyedService<IView>(key1);
        var view2 = serviceProvider.GetRequiredKeyedService<IView>(key2);

        Assert.NotNull(view1);
        Assert.NotNull(view2);
        Assert.NotSame(view1, view2);
        Assert.IsType<TestNavigationAware>(view1.DataContext);
        Assert.IsType<AnotherTestNavigationAware>(view2.DataContext);
    }

    [Fact]
    public void SameKey_DifferentRegistrationMethods_LastOneWins()
    {
        // Arrange
        var sameKey = "conflictingKey";

        // Act
        _services.RegisterView<TestView, TestMultiAwareViewModel>(sameKey);
        _services.RegisterNavigation<AnotherTestView, AnotherTestNavigationAware>(sameKey);

        var serviceProvider = _services.BuildServiceProvider();

        // Assert
        var keyedView = serviceProvider.GetRequiredKeyedService<IView>(sameKey);
        Assert.NotNull(keyedView);
        Assert.IsType<AnotherTestView>(keyedView);
    }

    [Fact]
    public void ServiceProvider_DisposesCorrectly()
    {
        // Arrange
        var viewKey = "disposableView";
        _services.RegisterView<TestView, TestNavigationAware>(viewKey);

        // Act & Assert
        using var serviceProvider = _services.BuildServiceProvider();
        var view = serviceProvider.GetRequiredKeyedService<IView>(viewKey);

        Assert.NotNull(view);
    }

    [Theory]
    [InlineData("stringKey")]
    [InlineData(123)]
    [InlineData('c')]
    [InlineData(1.5)]
    public void RegisterView_WithDifferentKeyTypes_WorksCorrectly(object viewKey)
    {
        // Act
        _services.RegisterView<TestView, TestNavigationAware>(viewKey);
        var serviceProvider = _services.BuildServiceProvider();

        // Assert
        var keyedView = serviceProvider.GetRequiredKeyedService<IView>(viewKey);
        Assert.NotNull(keyedView);
        Assert.IsType<TestView>(keyedView);
    }

    [Fact]
    public void RegisterView_DuplicateRegistration()
    {
        // Arrange
        var viewKey = "duplicateView";

        // Act
        _services.RegisterView<TestView, TestNavigationAware>(viewKey);
        _services.RegisterView<TestView, TestNavigationAware>(viewKey);
        _services.RegisterView<AnotherTestView, TestNavigationAware>(viewKey);
        _services.RegisterView<AnotherTestView, AnotherTestNavigationAware>(viewKey);

        var serviceProvider = _services.BuildServiceProvider();

        var views = serviceProvider.GetKeyedServices<TestView>(viewKey);
        Assert.True(views.Count() == 2);

        var anotherViews = serviceProvider.GetKeyedServices<AnotherTestView>(viewKey);
        Assert.True(anotherViews.Count() == 2);

        var keyedViews = serviceProvider.GetKeyedServices<IView>(viewKey);
        Assert.True(keyedViews.Count() == 4);

        var testNavigationAwares = serviceProvider.GetKeyedServices<TestNavigationAware>(viewKey);
        Assert.True(testNavigationAwares.Count() == 3);

        var anotherTestNavigationAwares = serviceProvider.GetKeyedServices<AnotherTestNavigationAware>(viewKey);
        Assert.True(anotherTestNavigationAwares.Count() == 1);

        var keyedAwares = serviceProvider.GetKeyedServices<INavigationAware>(viewKey);
        Assert.True(keyedAwares.Count() == 4);

        var keyedView = serviceProvider.GetRequiredKeyedService<IView>(viewKey);
        Assert.IsType<AnotherTestView>(keyedView);
        Assert.IsType<AnotherTestNavigationAware>(keyedView.DataContext);
    }

    [Fact]
    public void MultiRegistration()
    {
        // Arrange
        var registerViewKey = "RegisterView";
        var registerNavigationKey = "RegisterNavigation";
        var registerDialogKey = "RegisterDialog";

        // Act
        _services.RegisterView<TestView, TestMultiAwareViewModel>(registerViewKey);
        _services.RegisterNavigation<TestView, TestNavigationAware>(registerNavigationKey);
        _services.RegisterDialog<TestView, TestDialogAware>(registerDialogKey);

        var serviceProvider = _services.BuildServiceProvider();

        // Assert
        Assert.Single(serviceProvider.GetKeyedServices<TestView>(registerViewKey));
        Assert.Single(serviceProvider.GetKeyedServices<TestView>(registerNavigationKey));
        Assert.Single(serviceProvider.GetKeyedServices<TestView>(registerDialogKey));
        Assert.Single(serviceProvider.GetKeyedServices<IView>(registerViewKey));
        Assert.Single(serviceProvider.GetKeyedServices<IView>(registerNavigationKey));
        Assert.Single(serviceProvider.GetKeyedServices<IView>(registerDialogKey));
        Assert.IsType<TestMultiAwareViewModel>(serviceProvider.GetRequiredKeyedService<IView>(registerViewKey).DataContext);
        Assert.IsType<TestNavigationAware>(serviceProvider.GetRequiredKeyedService<IView>(registerNavigationKey).DataContext);
        Assert.IsType<TestDialogAware>(serviceProvider.GetRequiredKeyedService<IView>(registerDialogKey).DataContext);
    }

    [Fact]
    public void MultiRegistration_SameKey()
    {
        // Arrange
        var viewKey = "RegisterView";

        // Act
        _services.RegisterView<TestView, TestMultiAwareViewModel>(viewKey);
        _services.RegisterNavigation<TestView, TestNavigationAware>(viewKey);
        _services.RegisterDialog<TestView, TestDialogAware>(viewKey);

        var serviceProvider = _services.BuildServiceProvider();

        // Assert
        Assert.True(3 == serviceProvider.GetKeyedServices<TestView>(viewKey).Count());
        Assert.True(3 == serviceProvider.GetKeyedServices<IView>(viewKey).Count());
        Assert.IsNotType<TestMultiAwareViewModel>(serviceProvider.GetRequiredKeyedService<IView>(viewKey).DataContext);
        Assert.IsNotType<TestNavigationAware>(serviceProvider.GetRequiredKeyedService<IView>(viewKey).DataContext);
        Assert.IsType<TestDialogAware>(serviceProvider.GetRequiredKeyedService<IView>(viewKey).DataContext);
        Assert.IsNotType<TestNavigationAware>(serviceProvider.GetRequiredKeyedService<INavigationAware>(viewKey));
        Assert.IsNotType<TestDialogAware>(serviceProvider.GetRequiredKeyedService<IDialogAware>(viewKey));
    }
    #endregion
}
