using AsyncNavigation.Abstractions;
using Avalonia.Controls;
using Avalonia.Input.Platform;
using Avalonia.Platform.Storage;

namespace AsyncNavigation.Avalonia;

internal sealed class ViewContext : IViewContext
{
    public ViewContext(TopLevel topLevel)
    {
        TopLevel = topLevel;
    }

    public TopLevel TopLevel { get; }
    public IStorageProvider StorageProvider => TopLevel.StorageProvider;
    public IClipboard? Clipboard => TopLevel.Clipboard;
    public ILauncher? Launcher => TopLevel.Launcher;
}
