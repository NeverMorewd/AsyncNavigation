using AsyncNavigation.Abstractions;

namespace AsyncNavigation;

public class RegionNavigationHistory : IRegionNavigationHistory
{
    private readonly List<NavigationContext> _history = [];
    private int _currentIndex = -1;
    private readonly int _maxHistorySize;

    public RegionNavigationHistory(NavigationOptions navigationOptions)
    {
        _maxHistorySize = navigationOptions.MaxHistoryItems;
    }

    public bool CanGoBack => _currentIndex > 0;
    public bool CanGoForward => _currentIndex < _history.Count - 1;
    public IReadOnlyList<NavigationContext> History => _history.AsReadOnly();
    public NavigationContext? Current => _currentIndex >= 0 && _currentIndex < _history.Count
        ? _history[_currentIndex]
        : null;

    public void Add(NavigationContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        if (CanGoForward)
        {
            _history.RemoveRange(_currentIndex + 1, _history.Count - (_currentIndex + 1));
        }

        _history.Add(context);
        _currentIndex = _history.Count - 1;

        if (_history.Count > _maxHistorySize)
        {
            int removeCount = _history.Count - _maxHistorySize;
            _history.RemoveRange(0, removeCount);
            _currentIndex -= removeCount;
        }
    }

    public NavigationContext? GoBack()
    {
        if (!CanGoBack)
            return null;

        _currentIndex--;
        return _history[_currentIndex];
    }

    public NavigationContext? GoForward()
    {
        if (!CanGoForward)
            return null;

        _currentIndex++;
        return _history[_currentIndex];
    }

    public void Clear()
    {
        _history.Clear();
        _currentIndex = -1;
    }
}
