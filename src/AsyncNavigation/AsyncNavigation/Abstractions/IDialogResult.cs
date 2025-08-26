using AsyncNavigation.Core;

namespace AsyncNavigation.Abstractions;

public interface IDialogResult
{
    IDialogParameters? Parameters { get; }
    DialogButtonResult Result { get; }
    DialogResultStatus Status { get; }
}
