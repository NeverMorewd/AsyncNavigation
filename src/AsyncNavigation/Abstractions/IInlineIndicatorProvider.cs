namespace AsyncNavigation.Abstractions;

public interface IInlineIndicatorProvider
{
    bool HasIndicator(string regionName);
    IInlineIndicator GetIndicator(string regionName);
}
