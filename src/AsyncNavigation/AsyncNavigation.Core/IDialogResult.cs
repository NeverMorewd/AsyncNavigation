namespace AsyncNavigation.Core;

public interface IDialogResult
{
    IDialogParameters? Parameters { get; }
    DialogButtonResult Result { get; }
}
