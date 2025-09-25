namespace AsyncNavigation.Abstractions;

public interface IWindowBase
{
    string? Title { get; set; }
    object? Content { get; set; }
    object? DataContext { get; set; }

    event EventHandler? Closed;

    void Close();
    void Show();
}
