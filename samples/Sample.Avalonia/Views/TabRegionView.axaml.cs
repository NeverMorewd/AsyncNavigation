using AsyncNavigation.Abstractions;
using Avalonia.Controls;
using AsyncNavigation.Floating;
using Avalonia.Interactivity;
using System.Diagnostics;
using System;

namespace Sample.Avalonia.Views;

public partial class TabRegionView : UserControl, IView
{
    private readonly IViewPlacementService? _placementService;

    public TabRegionView() : this(null)
    {
    }

    public TabRegionView(IViewPlacementService? placementService)
    {
        _placementService = placementService;
        InitializeComponent();
    }

    private async void FloatCurrent_Click(object? sender, RoutedEventArgs e)
    {
        if (_placementService is null)
            return;

        try
        {
            await _placementService.FloatAsync("TabRegion", options: new FloatingWindowOptions
            {
                Title = "AsyncNavigation floating tab",
                Width = 720,
                Height = 480
            });
        }
        catch (Exception ex) when (ex is InvalidOperationException or NotSupportedException)
        {
            Debug.WriteLine($"Cannot float tab: {ex.Message}");
        }
    }
}
