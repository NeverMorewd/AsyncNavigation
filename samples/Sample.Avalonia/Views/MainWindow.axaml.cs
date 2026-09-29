using Avalonia.Controls;

using AsyncNavigation.Floating;

namespace Sample.Avalonia.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow() : this(null)
        {
        }

        public MainWindow(IViewPlacementService? placementService)
        {
            InitializeComponent();
            // MainView carries the whole app UI (including the "float MainRegion" demo), shared
            // as-is with the single-view (browser/mobile) lifetime in App.axaml.cs, so desktop
            // and single-view platforms show exactly the same floating samples.
            Content = new MainView(placementService);
        }
    }
}
