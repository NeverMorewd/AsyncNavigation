using AsyncNavigation;
using AsyncNavigation.Abstractions;
using AsyncNavigation.Core;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;

namespace Sample.Common;

public partial class HeavyViewModel : InstanceCounterViewModel<HeavyViewModel>, IDialogAware, INavigationMetadata
{
    public ObservableCollection<HeavyItemViewModel> HeavyItems
    {
        get;
    } = [];

    public string Title => $"{nameof(HeavyViewModel)}:{InstanceNumber}";

    public IconDescriptor Icon => IconDescriptor.FromPathData("M10 20v-6h4v6h5v-8h3L12 3 2 12h3v8z");

    public event AsyncEventHandler<DialogCloseEventArgs>? RequestCloseAsync;

    [RelayCommand]
    private Task UnloadView(string param)
    {
        return RequestUnloadAsync(CancellationToken.None);
    }

    [RelayCommand]
    private Task CloseDialog(string param)
    {
        return RequestCloseAsync!.Invoke(this, 
            new DialogCloseEventArgs(new DialogResult(DialogButtonResult.OK), CancellationToken.None));
    }
    public override Task InitializeAsync(NavigationContext context)
    {
        AddItems(20);
        return base.InitializeAsync(context);
    }
    public async Task OnDialogOpenedAsync(IDialogParameters? parameters, CancellationToken cancellationToken)
    {
        IsDialog = true;
        if (cancellationToken != CancellationToken.None)
        {
            await Task.Delay(TimeSpan.FromSeconds(5), cancellationToken);
        }
    }

    public Task OnDialogClosingAsync(IDialogResult? dialogResult, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    public Task OnDialogClosedAsync(IDialogResult? dialogResult, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    private void AddItems(int count)
    {
        var rnd = new Random();
        for (int i = 0; i < count; i++)
        {
            HeavyItems.Add(new HeavyItemViewModel((byte)rnd.Next(0, 256)));
        }
    }
}

public partial class HeavyItemViewModel(byte value) : ObservableObject
{
    public byte Value { get; } = value;

    public IReadOnlyList<string> Options { get; } = ["Alpha", "Beta", "Gamma"];

    [ObservableProperty]
    private string? _selectedOption;

    [ObservableProperty]
    private DateTimeOffset? _avaloniaSelectedDate;

    [ObservableProperty]
    private DateTime? _wpfSelectedDate;

    [ObservableProperty]
    private bool _isExpanded;
}
