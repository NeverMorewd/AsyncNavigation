using AsyncNavigation.Core;

namespace AsyncNavigation.Abstractions;

public interface IDialogResult
{
    IDialogParameters? Parameters { get; }
    DialogButtonResult Result { get; }
    DialogStatus Status { get; }
}
