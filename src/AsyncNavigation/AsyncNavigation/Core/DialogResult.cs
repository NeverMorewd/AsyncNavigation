using AsyncNavigation.Abstractions;

namespace AsyncNavigation.Core;

public class DialogResult : IDialogResult
{
    public DialogResult()
    {

    }

    public DialogResult(DialogButtonResult result)
    {
        Result = result;
        Status = DialogResultStatus.Closed;
    }

    public DialogResult(DialogButtonResult result, IDialogParameters parameters)
    {
        Result = result;
        Parameters = parameters;
        Status = DialogResultStatus.Closed;
    }

    public static DialogResult Cancelled => new(DialogButtonResult.None) { Status = DialogResultStatus.Cancelled };

    public IDialogParameters? Parameters
    {
        get;
        private set;
    }

    public DialogButtonResult Result
    {
        get;
        private set;
    } = DialogButtonResult.None;

    public DialogResultStatus Status
    {
        get;
        private set;
    } 
}
