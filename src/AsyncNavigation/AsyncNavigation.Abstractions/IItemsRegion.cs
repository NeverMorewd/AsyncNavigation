using AsyncNavigation.Core;
using System.Collections.ObjectModel;

namespace AsyncNavigation.Abstractions;

public interface IItemsRegion<T> : IRegion
{
    ObservableCollection<NavigationContext> Contexts
    {
        get;
    }
    T ItemsControl { get; }
}
