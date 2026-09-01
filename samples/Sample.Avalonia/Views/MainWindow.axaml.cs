using Avalonia.Controls;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

using AsyncNavigation.Floating;
using Avalonia.Interactivity;

namespace Sample.Avalonia.Views
{
    public partial class MainWindow : Window
    {
        private readonly IViewPlacementService? _placementService;

        public MainWindow() : this(null)
        {
        }

        public MainWindow(IViewPlacementService? placementService)
        {
            _placementService = placementService;
            InitializeComponent();
        }

        private async void FloatMainRegion_Click(object? sender, RoutedEventArgs e)
        {
            if (_placementService is null)
                return;

            try
            {
                await _placementService.FloatAsync("MainRegion", options: new FloatingWindowOptions
                {
                    Title = "AsyncNavigation floating MainRegion",
                    Width = 900,
                    Height = 600
                });
            }
            catch (Exception ex) when (ex is InvalidOperationException or NotSupportedException)
            {
                Debug.WriteLine($"Cannot float MainRegion: {ex.Message}");
            }
        }
    }
}
