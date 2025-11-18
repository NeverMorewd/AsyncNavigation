using AsyncNavigation.Abstractions;
using AsyncNavigation.Core;
using AsyncNavigation.Tests.Mocks;
using Microsoft.Extensions.DependencyInjection;
using Xunit.Abstractions;

namespace AsyncNavigation.Tests;

public class ServiceCollectionExtensionsTests
{
    private readonly ITestOutputHelper _testOutputHelper;

    public ServiceCollectionExtensionsTests(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }

    #region RegisterView Tests

    [Fact]
    public void RegisterView_ValidParameters_RegistersServicesCorrectly()
    {
        // Arrange
        var viewKey = "testView";
        var services = new ServiceCollection();
        // Act
        services.RegisterView<TestView, TestNavigationAware>(viewKey);
        var serviceProvider = services.BuildServiceProvider();

        // Assert
        var view = serviceProvider.GetRequiredKeyedService<IView>(viewKey);
        var viewModel = serviceProvider.GetRequiredKeyedService<INavigationAware>(viewKey);
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
        string viewKey = null!;
        var services = new ServiceCollection();
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            services.RegisterView<TestView, TestNavigationAware>(viewKey));
    }

    [Fact]
    public void RegisterView_ViewModelWithoutRequiredInterface_ThrowsInvalidOperationException()
    {
        // Arrange
        var viewKey = "invalidView";
        var services = new ServiceCollection();
        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() =>
            services.RegisterView<TestView, TestView>(viewKey));

        Assert.Contains("must implement at least one of the following interfaces", exception.Message);
    }

    [Fact]
    public void RegisterView_DataContextIsSetCorrectly()
    {
        // Arrange
        var viewKey = "dataContextView";
        var services = new ServiceCollection();
        // Act
        services.RegisterView<TestView, TestNavigationAware>(viewKey);
        var serviceProvider = services.BuildServiceProvider();

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
        var services = new ServiceCollection();
        // Act
        services.RegisterView<TestView, TestNavigationAware>(viewKey1);
        services.RegisterView<TestView, AnotherTestNavigationAware>(viewKey2);

        var serviceProvider = services.BuildServiceProvider();

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
        var services = new ServiceCollection();
        // Act
        services.RegisterView<TestView, TestMultiAwareViewModel>(viewKey);
        var serviceProvider = services.BuildServiceProvider();

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
        var services = new ServiceCollection();

        // Act
        services.RegisterNavigation<TestView, TestNavigationAware>(viewKey);
        var serviceProvider = services.BuildServiceProvider();

        // Assert
        var view = serviceProvider.GetRequiredService<TestView>();
        var viewModel = serviceProvider.GetRequiredService<TestNavigationAware>();
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
        TestNavigationAware viewModelBuilder(IServiceProvider sp) => customViewModel;
        var services = new ServiceCollection();

        // Act
        services.RegisterNavigation<TestView, TestNavigationAware>(viewKey, viewModelBuilder);
        var serviceProvider = services.BuildServiceProvider();

        // Assert
        var keyedViewModel = serviceProvider.GetRequiredKeyedService<INavigationAware>(viewKey);
        var keyedView = serviceProvider.GetRequiredKeyedService<IView>(viewKey);

        Assert.Same(customViewModel, keyedViewModel);
        Assert.NotNull(keyedView);
        Assert.IsType<TestNavigationAware>(keyedView.DataContext);
    }

    [Fact(Skip = "allow null build for now")]
    public void RegisterNavigation_WithViewModelBuilder_NullBuilder_ThrowsArgumentNullException()
    {
        // Arrange
        var viewKey = "navigationView";
        Func<IServiceProvider, TestNavigationAware> viewModelBuilder = null!;
        var services = new ServiceCollection();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            services.RegisterNavigation<TestView, TestNavigationAware>(viewKey, viewModelBuilder));
    }

    [Fact]
    public void RegisterNavigation_DuplicateKey_DifferentViewTypes_RegistersBoth()
    {
        // Arrange
        var sameKey = "sameKey";
        var services = new ServiceCollection();

        // Act
        services.RegisterNavigation<TestView, TestNavigationAware>(sameKey);
        services.RegisterNavigation<AnotherTestView, AnotherTestNavigationAware>(sameKey);

        var serviceProvider = services.BuildServiceProvider();

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
        TestNavigationAware viewModelBuilder(IServiceProvider sp) => customViewModel;
        var services = new ServiceCollection();

        // Act
        services.RegisterNavigation<TestView, TestNavigationAware>(viewKey, viewModelBuilder);
        var serviceProvider = services.BuildServiceProvider();

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
        var services = new ServiceCollection();
        // Act
        services.RegisterDialog<TestView, TestDialogAware>(viewKey);
        var serviceProvider = services.BuildServiceProvider();

        // Assert
        var view = serviceProvider.GetRequiredService<TestView>();
        var viewModel = serviceProvider.GetRequiredService<TestDialogAware>();
        var dialogAware = serviceProvider.GetRequiredKeyedService<IDialogAware>(viewKey);
        var keyedView = serviceProvider.GetRequiredKeyedService<IView>(viewKey);

        Assert.NotNull(view);
        Assert.NotNull(viewModel);
        Assert.NotNull(keyedView);
        Assert.IsType<TestDialogAware>(keyedView.DataContext);
        Assert.IsType<TestDialogAware>(dialogAware);
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
        TestDialogAware viewModelBuilder(IServiceProvider sp) => customViewModel;
        var services = new ServiceCollection();
        // Act
        services.RegisterDialog<TestView, TestDialogAware>(viewKey, viewModelBuilder);
        var serviceProvider = services.BuildServiceProvider();

        // Assert
        var keyedViewModel = serviceProvider.GetRequiredKeyedService<IDialogAware>(viewKey);
        var keyedView = serviceProvider.GetRequiredKeyedService<IView>(viewKey);

        Assert.Same(customViewModel, keyedViewModel);
        Assert.NotNull(keyedView);
        Assert.Same(customViewModel, keyedView.DataContext);
    }

    [Fact(Skip = "allow null builder for now")]
    public void RegisterDialog_WithViewModelBuilder_NullBuilder_ThrowsArgumentNullException()
    {
        // Arrange
        var viewKey = "dialogView";
        Func<IServiceProvider, TestDialogAware> viewModelBuilder = null!;
        var services = new ServiceCollection();
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            services.RegisterDialog<TestView, TestDialogAware>(viewKey, viewModelBuilder));
    }

    [Fact]
    public void RegisterDialog_MixedRegistrations_SameKey_DifferentTypes()
    {
        // Arrange
        var mixedKey = "mixedKey";
        var services = new ServiceCollection();
        // Act
        services.RegisterView<TestView, TestMultiAwareViewModel>(mixedKey);
        services.RegisterNavigation<AnotherTestView, AnotherTestNavigationAware>(mixedKey);
        services.RegisterDialog<TestView, TestDialogAware>(mixedKey);

        var serviceProvider = services.BuildServiceProvider();

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
        TestDialogAware viewModelBuilder(IServiceProvider sp) => customViewModel;
        var services = new ServiceCollection();
        // Act
        services.RegisterDialog<TestView, TestDialogAware>(viewKey, viewModelBuilder);
        var serviceProvider = services.BuildServiceProvider();

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
        var services = new ServiceCollection();

        // Act
        services.RegisterView<TestView, TestNavigationAware>(key1);
        services.RegisterView<TestView, AnotherTestNavigationAware>(key2);

        var serviceProvider = services.BuildServiceProvider();

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
        var services = new ServiceCollection();

        // Act
        services.RegisterView<TestView, TestMultiAwareViewModel>(sameKey);
        services.RegisterNavigation<AnotherTestView, AnotherTestNavigationAware>(sameKey);

        var serviceProvider = services.BuildServiceProvider();

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
        var services = new ServiceCollection();
        services.RegisterView<TestView, TestNavigationAware>(viewKey);

        // Act & Assert
        using var serviceProvider = services.BuildServiceProvider();
        var view = serviceProvider.GetRequiredKeyedService<IView>(viewKey);

        Assert.NotNull(view);
    }

    [Theory]
    [InlineData("stringKey")]
    [InlineData("123")]
    [InlineData("1.5")]
    public void RegisterView_WithDifferentKeyTypes_WorksCorrectly(string viewKey)
    {
        // Act
        var services = new ServiceCollection();
        services.RegisterView<TestView, TestNavigationAware>(viewKey);
        var serviceProvider = services.BuildServiceProvider();

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
        var services = new ServiceCollection();

        // Act
        services.RegisterView<TestView, TestNavigationAware>(viewKey);
        services.RegisterView<TestView, TestNavigationAware>(viewKey);
        services.RegisterView<AnotherTestView, TestNavigationAware>(viewKey);
        services.RegisterView<AnotherTestView, AnotherTestNavigationAware>(viewKey);

        var serviceProvider = services.BuildServiceProvider();

        var views = serviceProvider.GetServices<TestView>();
        Assert.True(views.Count() == 2);

        var anotherViews = serviceProvider.GetServices<AnotherTestView>();
        Assert.True(anotherViews.Count() == 2);

        var keyedViews = serviceProvider.GetKeyedServices<IView>(viewKey);
        Assert.True(keyedViews.Count() == 4);

        var testNavigationAwares = serviceProvider.GetServices<TestNavigationAware>();
        Assert.True(testNavigationAwares.Count() == 3);

        var anotherTestNavigationAwares = serviceProvider.GetServices<AnotherTestNavigationAware>();
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
        var services = new ServiceCollection();

        // Act
        services.RegisterView<TestView, TestMultiAwareViewModel>(registerViewKey);
        services.RegisterNavigation<TestView, TestNavigationAware>(registerNavigationKey);
        services.RegisterDialog<TestView, TestDialogAware>(registerDialogKey);

        var serviceProvider = services.BuildServiceProvider();

        // Assert
        Assert.Equal(3, serviceProvider.GetServices<TestView>().Count());
        Assert.Empty(serviceProvider.GetKeyedServices<TestView>(registerViewKey));
        Assert.Empty(serviceProvider.GetKeyedServices<TestView>(registerNavigationKey));
        Assert.Empty(serviceProvider.GetKeyedServices<TestView>(registerDialogKey));
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
        var services = new ServiceCollection();

        // Act
        services.RegisterView<TestView, TestMultiAwareViewModel>(viewKey);
        services.RegisterNavigation<TestView, TestNavigationAware>(viewKey);
        services.RegisterDialog<TestView, TestDialogAware>(viewKey);

        var serviceProvider = services.BuildServiceProvider();

        // Assert
        Assert.True(3 == serviceProvider.GetServices<TestView>().Count());
        Assert.True(3 == serviceProvider.GetKeyedServices<IView>(viewKey).Count());

        var finalVM = serviceProvider.GetRequiredKeyedService<IView>(viewKey).DataContext;

        Assert.IsNotType<TestMultiAwareViewModel>(finalVM);
        Assert.IsNotType<TestNavigationAware>(finalVM);
        Assert.IsType<TestDialogAware>(finalVM);
        Assert.IsNotType<TestMultiAwareViewModel>(serviceProvider.GetRequiredKeyedService<INavigationAware>(viewKey));
        Assert.IsType<TestNavigationAware>(serviceProvider.GetRequiredKeyedService<INavigationAware>(viewKey));
        Assert.IsType<TestDialogAware>(serviceProvider.GetRequiredKeyedService<IDialogAware>(viewKey));
    }
    #endregion

    #region RegisterFramework Tests
    [Fact]
    public void RegisterNavigationFramework_Default()
    {
        // Arrange
        var services = new ServiceCollection();
        services.RegisterNavigationFramework();
        services.AddSingleton<IPlatformService, TestPlatformService>();
        services.AddTransient<IInnerRegionIndicatorHost, TestInnerRegionIndicatorHost>();
        var serviceProvider = services.BuildServiceProvider();
        // Act

        var options = serviceProvider.GetRequiredService<NavigationOptions>();

        var jobProcessor1 = serviceProvider.GetRequiredService<IAsyncJobProcessor>();
        var jobProcessor2 = serviceProvider.GetRequiredService<IAsyncJobProcessor>();

        var dialogService1 = serviceProvider.GetRequiredService<IDialogService>();
        var dialogService2 = serviceProvider.GetRequiredService<IDialogService>();

        var regionNavigationServiceFactory1 = serviceProvider.GetRequiredService<IRegionNavigationServiceFactory>();
        var regionNavigationServiceFactory2 = serviceProvider.GetRequiredService<IRegionNavigationServiceFactory>();

        var regionFactory1 = serviceProvider.GetRequiredService<IRegionFactory>();
        var regionFactory2 = serviceProvider.GetRequiredService<IRegionFactory>();

        var viewFactory1 = serviceProvider.GetRequiredService<IViewFactory>();
        var viewFactory2 = serviceProvider.GetRequiredService<IViewFactory>();

        var viewManager1 = serviceProvider.GetRequiredService<IViewManager>();
        var viewManager2 = serviceProvider.GetRequiredService<IViewManager>();

        var regionNavigationHistory1 = serviceProvider.GetRequiredService<IRegionNavigationHistory>();
        var regionNavigationHistory2 = serviceProvider.GetRequiredService<IRegionNavigationHistory>();

        var regionIndicatorProvider1 = serviceProvider.GetRequiredService<IRegionIndicatorProvider>();
        var regionIndicatorProvider2 = serviceProvider.GetRequiredService<IRegionIndicatorProvider>();

        var regionIndicatorManager1 = serviceProvider.GetRequiredService<IRegionIndicatorManager>();
        var regionIndicatorManager2 = serviceProvider.GetRequiredService<IRegionIndicatorManager>();

        //Assert
        Assert.NotNull(jobProcessor1);
        if (options.NavigationJobScope == Core.NavigationJobScope.Region)
        {
            _testOutputHelper.WriteLine("NavigationJobScope is Region - expecting transient IAsyncJobProcessor instances.");
            Assert.NotEqual(jobProcessor1, jobProcessor2);
        }
        else
        {
            _testOutputHelper.WriteLine("NavigationJobScope is App - expecting singleton IAsyncJobProcessor instances.");
            Assert.Equal(jobProcessor1, jobProcessor2);
        }

            Assert.NotNull(dialogService1);
        Assert.Equal(dialogService1, dialogService2);

        Assert.NotNull(regionNavigationServiceFactory1);
        Assert.Equal(regionNavigationServiceFactory1, regionNavigationServiceFactory2);

        Assert.NotNull(regionFactory1);
        Assert.Equal(regionFactory1, regionFactory2);

        Assert.NotNull(viewFactory1);
        Assert.Equal(viewFactory1, viewFactory2);

        Assert.NotNull(viewManager1);
        Assert.NotNull(viewManager2);
        Assert.NotEqual(viewManager1, viewManager2);

        Assert.NotNull(regionNavigationHistory1);
        Assert.NotNull(regionNavigationHistory2);
        Assert.NotEqual(regionNavigationHistory1, regionNavigationHistory2);

        Assert.NotNull(regionIndicatorProvider1);
        Assert.NotNull(regionIndicatorProvider2);
        Assert.NotEqual(regionIndicatorProvider1, regionIndicatorProvider2);

        Assert.NotNull(regionIndicatorManager1);
        Assert.NotNull(regionIndicatorManager2);
        Assert.NotEqual(regionIndicatorManager1, regionIndicatorManager2);
    }

    [Fact]
    public void RegisterNavigationFramework_CustomOptions_NavigationJobScopeApp()
    {
        // Arrange
        var services = new ServiceCollection();
        services.RegisterNavigationFramework(new NavigationOptions { NavigationJobScope = Core.NavigationJobScope.App });
        services.AddSingleton<IPlatformService, TestPlatformService>();
        services.AddTransient<IInnerRegionIndicatorHost, TestInnerRegionIndicatorHost>();
        var serviceProvider = services.BuildServiceProvider();
        // Act
        var jobProcessor1 = serviceProvider.GetRequiredService<IAsyncJobProcessor>();
        var jobProcessor2 = serviceProvider.GetRequiredService<IAsyncJobProcessor>();
     
        //Assert
        Assert.NotNull(jobProcessor1);
        Assert.Equal(jobProcessor1, jobProcessor2);
    }

    [Fact]
    public void RegisterNavigationFramework_CustomOptions_NavigationJobScopeRegion()
    {
        // Arrange
        var services = new ServiceCollection();
        services.RegisterNavigationFramework(new NavigationOptions { NavigationJobScope = Core.NavigationJobScope.Region });
        services.AddSingleton<IPlatformService, TestPlatformService>();
        services.AddTransient<IInnerRegionIndicatorHost, TestInnerRegionIndicatorHost>();
        var serviceProvider = services.BuildServiceProvider();
        // Act
        var jobProcessor1 = serviceProvider.GetRequiredService<IAsyncJobProcessor>();
        var jobProcessor2 = serviceProvider.GetRequiredService<IAsyncJobProcessor>();

        //Assert
        Assert.NotNull(jobProcessor1);
        Assert.NotNull(jobProcessor2);
        Assert.NotEqual(jobProcessor1, jobProcessor2);
    }

    [Fact]
    public void RegisterNavigationFramework_CustomOptions()
    {
        // Arrange
        var services = new ServiceCollection();
        var options = new NavigationOptions
        {
            MaxCachedViews = 20,
            MaxHistoryItems = 30,
            MaxReplayItems = 15,
            LoadingIndicatorDelay = TimeSpan.FromMilliseconds(500),
            NavigationJobStrategy = Core.NavigationJobStrategy.Queue,
            NavigationJobScope = Core.NavigationJobScope.App,
            ViewCacheStrategy = Core.ViewCacheStrategy.UpdateDuplicateKey
        };

        services.RegisterNavigationFramework(options);
        services.AddSingleton<IPlatformService, TestPlatformService>();
        services.AddTransient<IInnerRegionIndicatorHost, TestInnerRegionIndicatorHost>();
        var serviceProvider = services.BuildServiceProvider();
        // Act
        var actualOptions = serviceProvider.GetRequiredService<NavigationOptions>();

        //Assert
        Assert.NotNull(actualOptions);
        Assert.Equal(actualOptions, options);
    }

    #endregion

    #region RegisterDialogWindow Tests
    [Fact]
    public void RegisterDialogWindow_Default()
    {
        // Arrange
        var windowName = "RegisterDialogWindow";
        var services = new ServiceCollection();

        // Act
        services.RegisterDialogWindow<TestDialogWindow, TestDialogAware>(windowName);

        var serviceProvider = services.BuildServiceProvider();
        var window = serviceProvider.GetRequiredService<TestDialogWindow>();
        var viewModel = serviceProvider.GetRequiredService<TestDialogAware>();
        var dialogWindow = serviceProvider.GetRequiredKeyedService<IDialogWindow>(windowName);
        var dataContext = dialogWindow.DataContext;
        var dialogAware = serviceProvider.GetRequiredKeyedService<IDialogAware>(windowName);
        // Assert
        Assert.True(1 == serviceProvider.GetServices<TestDialogWindow>().Count());

        Assert.IsAssignableFrom<IDialogWindow>(window);
        Assert.IsAssignableFrom<IDialogAware>(viewModel);
        Assert.IsType<TestDialogAware>(dataContext);
        Assert.IsType<TestDialogAware>(dialogAware);
    }
    [Fact]
    public void RegisterNavigation_And_RegisterDialog_Should_Track_Category()
    {
        var services = new ServiceCollection();
        services.RegisterNavigationFramework();
        // Act
        services.RegisterNavigation<DummyNavigationView, DummyNavigationViewModel>("NavView");
        services.RegisterDialog<DummyDialogView, DummyDialogViewModel>("DialogView");
        services.RegisterView<DummyComboView, DummyComboViewModel>("ComboView");

        var provider = services.BuildServiceProvider();

        var tracker = (provider.GetRequiredService<IRegistrationTracker>() as RegistrationTracker)!;
        var keys = tracker.GetAll();
        foreach (var kvp in keys)
        {
            _testOutputHelper.WriteLine($"{kvp.Key}:");
            _testOutputHelper.WriteLine($"{string.Join(";", kvp.Value)}");
        }

        Assert.Contains("NavView", tracker.GetAll()[RegistryCategory.Navigation]);
        Assert.Contains("DialogView", tracker.GetAll()[RegistryCategory.Dialog]);

        Assert.Contains("ComboView", tracker.GetAll()[RegistryCategory.Navigation]);
        Assert.Contains("ComboView", tracker.GetAll()[RegistryCategory.Dialog]);
        Assert.Contains("ComboView", tracker.GetAll()[RegistryCategory.View]);

        _ = tracker.TryGetViews(out var intersection);
        Assert.Single(intersection!);
        Assert.Contains("ComboView", intersection!);

        _ = tracker.TryGetNavigations(out var navs);
        Assert.Contains("NavView", navs!);
        Assert.Contains("ComboView", navs!);

        _ = tracker.TryGetDialogs(out var dialogs);
        Assert.Contains("DialogView", dialogs!);
        Assert.Contains("ComboView", dialogs!);

        var navView = provider.GetRequiredKeyedService<IView>("NavView");
        Assert.IsType<DummyNavigationView>(navView);

        var comboView = provider.GetRequiredKeyedService<IView>("ComboView");
        Assert.IsType<DummyComboView>(comboView);
    }
    #endregion
}
