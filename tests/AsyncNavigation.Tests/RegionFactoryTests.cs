using AsyncNavigation;
using AsyncNavigation.Abstractions;
using AsyncNavigation.Tests.Mocks;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Xunit;

namespace AsyncNavigation.Tests;

public class RegionFactoryTests
{
    [Fact]
    public void Constructor_WithNullAdapters_ShouldInitializeEmptyList()
    {
        // Act
        var factory = new RegionFactory(null);

        // Assert
        var adapters = GetAdaptersField(factory);
        Assert.Empty(adapters);
    }

    [Fact]
    public void Constructor_WithEmptyAdapters_ShouldInitializeEmptyList()
    {
        // Arrange
        var adapters = Enumerable.Empty<IRegionAdapter>();

        // Act
        var factory = new RegionFactory(adapters);

        // Assert
        var internalAdapters = GetAdaptersField(factory);
        Assert.Empty(internalAdapters);
    }

    [Fact]
    public void RegisterAdapter_Null_ThrowsArgumentNullException()
    {
        // Arrange
        var factory = new RegionFactory(null);

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => factory.RegisterAdapter(null!));
    }

    [Fact]
    public void RegisterAdapter_ValidAdapter_AddsToInternalList()
    {
        // Arrange
        var factory = new RegionFactory(null);
        var mockAdapter = new Mock<IRegionAdapter>();
        mockAdapter.Setup(a => a.Priority).Returns((uint)0);

        // Act
        factory.RegisterAdapter(mockAdapter.Object);

        // Assert
        var adapters = GetAdaptersField(factory);
        Assert.Single(adapters);
        Assert.Same(mockAdapter.Object, adapters[0]);
    }

    [Fact]
    public void GetAdapter_SelectsHighestPriorityAndThenByName()
    {
        var control = new object();

        var adapterLow = new Mock<IRegionAdapter>();
        adapterLow.Setup(a => a.Priority).Returns((uint)1);
        adapterLow.Setup(a => a.IsAdapted(control)).Returns(true);

        var adapterHigh1 = new Mock<IRegionAdapter>();
        adapterHigh1.Setup(a => a.Priority).Returns((uint)2);
        adapterHigh1.Setup(a => a.IsAdapted(control)).Returns(true);

        var adapterHigh2 = new Mock<IRegionAdapter>();
        adapterHigh2.Setup(a => a.Priority).Returns((uint)2);
        adapterHigh2.Setup(a => a.IsAdapted(control)).Returns(true);

        var factory = new RegionFactory([
            adapterLow.Object,
        adapterHigh1.Object,
        adapterHigh2.Object
        ]);

        var selected = GetSelectedAdapterViaReflection(factory, control);

        Assert.Same(adapterHigh1.Object, selected);
    }


    [Fact]
    public void CreateRegion_NoMatchingAdapter_ThrowsNotSupportedException()
    {
        // Arrange
        var factory = new RegionFactory(null);
        var control = new object();

        // Act & Assert
        var ex = Assert.Throws<NotSupportedException>(() =>
            factory.CreateRegion("test", control, Mock.Of<IServiceProvider>()));

        Assert.Contains("No adapter found for control type: Object", ex.Message);
    }

    [Fact]
    public void CreateRegion_MatchingAdapter_CallsCreateRegionOnAdapter()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddNavigationTestSupport();
        var control = new object();
        var mockAdapter = new Mock<IRegionAdapter>();
        mockAdapter.Setup(a => a.Priority).Returns((uint)0);
        mockAdapter.Setup(a => a.IsAdapted(control)).Returns(true);

        var serviceProvider = services.BuildServiceProvider();
        var mockRegion = TestRegion.GetOne(serviceProvider);


        mockAdapter.Setup(a => a.CreateRegion("test", control, serviceProvider, null))
                   .Returns(mockRegion);

        var factory = new RegionFactory([mockAdapter.Object]);

        // Act
        var region = factory.CreateRegion("test", control, serviceProvider);

        // Assert
        Assert.Same(mockRegion, region);
        mockAdapter.Verify(a => a.CreateRegion("test", control, serviceProvider, null), Times.Once);
    }

    // --- Helper Methods ---

    private static ImmutableArray<IRegionAdapter> GetAdaptersField(RegionFactory factory)
    {
        var field = typeof(RegionFactory).GetField("_adapters",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        return (ImmutableArray<IRegionAdapter>)field!.GetValue(factory)!;
    }

    private static IRegionAdapter? GetSelectedAdapterViaReflection(RegionFactory factory, object control)
    {
        var method = typeof(RegionFactory).GetMethod("GetAdapter",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        return (IRegionAdapter?)method!.Invoke(factory, new object[] { control });
    }

    // --- Dummy Adapter Implementations for Ordering Test ---

    private sealed class AdapterHighA : IRegionAdapter
    {
        public uint Priority => 2;
        public bool IsAdapted(object control) => true;
        public IRegion CreateRegion(string name, object control, IServiceProvider serviceProvider, bool? useCache) =>
            throw new NotImplementedException();
    }

    private sealed class AdapterHighB : IRegionAdapter
    {
        public uint Priority => 2;
        public bool IsAdapted(object control) => true;
        public IRegion CreateRegion(string name, object control, IServiceProvider serviceProvider, bool? useCache) =>
            throw new NotImplementedException();
    }

    private sealed class AdapterLow : IRegionAdapter
    {
        public uint Priority => 1;
        public bool IsAdapted(object control) => true;
        public IRegion CreateRegion(string name, object control, IServiceProvider serviceProvider, bool? useCache) =>
            throw new NotImplementedException();
    }
}