using AsyncNavigation.Abstractions;

namespace AsyncNavigation.Tests.Mocks;

internal class TestDialogWindow : IDialogWindow
{
    public string? Title { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    public object? Content { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    public object? DataContext { get; set; }

    public event EventHandler? Closed;

    public void Close()
    {
        throw new NotImplementedException();
    }

    public void Show()
    {
        throw new NotImplementedException();
    }
}
