using System.Collections.ObjectModel;
using System.ComponentModel;

namespace AsyncNavigation.Core;

public sealed class RegionContext : INotifyPropertyChanged
{
    public ObservableCollection<NavigationContext> Items { get; } = [];

    private NavigationContext? _selected;
    public NavigationContext? Selected
    {
        get => _selected;
        set
        {
            if (!ReferenceEquals(_selected, value))
            {
                _selected = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Selected)));
            }
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public void Clear()
    {
        Items.Clear();
        Selected = null;
    }
}

