namespace AsyncNavigation.Core;

public static class NavigationDiagnostics
{
    public static event Action<Exception, string>? UnhandledException;

    internal static void Report(Exception ex, string message)
    {
        UnhandledException?.Invoke(ex, message);
    }
}
