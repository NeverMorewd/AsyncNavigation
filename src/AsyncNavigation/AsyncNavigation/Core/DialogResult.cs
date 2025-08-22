using AsyncNavigation.Abstractions;

namespace AsyncNavigation.Core;

public class DialogResult : IDialogResult
{
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

    public DialogResult()
    {
    }

    public DialogResult(DialogButtonResult result)
    {
        Result = result;
    }

    public DialogResult(DialogButtonResult result, IDialogParameters parameters)
    {
        Result = result;
        Parameters = parameters;
    }
}
