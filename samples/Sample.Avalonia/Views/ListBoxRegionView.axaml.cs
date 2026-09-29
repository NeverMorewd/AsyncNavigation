using AsyncNavigation.Abstractions;
using AsyncNavigation.Floating;
using Avalonia.Controls;
using Avalonia.Interactivity;
using System;
using System.Diagnostics;

namespace Sample.Avalonia.Views;

public partial class ListBoxRegionView : UserControl, IView
{
    private readonly IViewPlacementService? _placementService;

    public ListBoxRegionView() : this(null)
    {
    }

    public ListBoxRegionView(IViewPlacementService? placementService)
    {
        _placementService = placementService;
        InitializeComponent();
    }

    private async void FloatSelectedItem_Click(object? sender, RoutedEventArgs e)
    {
        if (_placementService is null)
            return;

        try
        {
            await _placementService.FloatAsync("CustomListBoxRegion", options: new FloatingWindowOptions
            {
                Title = "AsyncNavigation floating CustomListBoxRegion item",
                Width = 720,
                Height = 480
            });
        }
        catch (Exception ex) when (ex is InvalidOperationException or NotSupportedException)
        {
            Debug.WriteLine($"Cannot float CustomListBoxRegion: {ex.Message}");
        }
    }
}
