namespace AsyncNavigation.Abstractions;

public interface IDialogWindowBase
{
    string? Title { get; set; }
    object? Content { get; set; }
    object? DataContext { get; set; }
    void Close();
    void Show();
}
