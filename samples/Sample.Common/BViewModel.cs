using AsyncNavigation.Core;

namespace Sample.Common;

public class BViewModel : ViewModelBase
{
    public override async Task OnNavigatedToAsync(NavigationContext context,  CancellationToken cancellationToken)
    {
        await base.OnNavigatedToAsync(context, cancellationToken);
        await Task.Delay(5000, cancellationToken);
        //throw new InvalidOperationException("This is a test exception from BViewModel");
    }
}
