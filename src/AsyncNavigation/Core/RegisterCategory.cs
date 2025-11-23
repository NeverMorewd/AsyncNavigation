namespace AsyncNavigation.Core;

[Flags]
public enum RegistryCategory
{
    None = 0,
    Navigation = 1 << 0,
    Dialog = 1 << 1,
    View = Navigation | Dialog
}
