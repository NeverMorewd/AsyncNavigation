using AsyncNavigation.Abstractions;

namespace AsyncNavigation.Core;

public class DialogCloseEventArgs : AsyncEventArgs
{
    public IDialogResult DialogResult { get; private set; }
    public DialogCloseEventArgs(IDialogResult dialogResult, CancellationToken token) : base(token)
    {
        DialogResult = dialogResult;
    }
}
