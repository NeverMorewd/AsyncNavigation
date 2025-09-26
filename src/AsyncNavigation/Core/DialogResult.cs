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
        Status = DialogStatus.Closed;
    }

    public DialogResult(DialogButtonResult result, IDialogParameters parameters)
    {
        Result = result;
        Parameters = parameters;
        Status = DialogStatus.Closed;
    }

    public static DialogResult Cancelled => new(DialogButtonResult.None) { Status = DialogStatus.Cancelled };

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

    public DialogStatus Status
    {
        get;
        private set;
    } 
}
