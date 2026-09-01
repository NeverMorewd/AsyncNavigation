using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Headless.XUnit;
using Avalonia.LogicalTree;
using Avalonia.Threading;
using Avalonia.VisualTree;

namespace AsyncNavigation.E2E.Tests;

public sealed class FloatingVisualStateTests
{
    [AvaloniaFact]
    public void MovingSameViewBetweenWindows_PreservesTemplatedDatePickerState()
    {
        var state = new DatePickerState();
        var presenter = new ContentPresenter
        {
            Content = "item",
            ContentTemplate = new FuncDataTemplate<string>((_, _) =>
            {
                var picker = new DatePicker();
                picker.Bind(DatePicker.SelectedDateProperty, new Binding(nameof(DatePickerState.SelectedDate))
                {
                    Source = state,
                    Mode = BindingMode.TwoWay
                });
                return picker;
            })
        };
        var view = new UserControl { Content = presenter };
        var indicatorHost = new ContentControl { Content = view };
        var origin = new Window { Width = 800, Height = 600, Content = indicatorHost };
        var floating = new Window { Width = 800, Height = 600 };

        try
        {
            origin.Show();
            Dispatcher.UIThread.RunJobs(DispatcherPriority.Loaded);
            view.Measure(new Size(800, 600));
            view.Arrange(new Rect(0, 0, 800, 600));
            Dispatcher.UIThread.RunJobs();

            var originalPicker = view.GetVisualDescendants().OfType<DatePicker>().Single();
            var selectedDate = new DateTimeOffset(2026, 9, 1, 0, 0, 0, TimeSpan.Zero);
            originalPicker.SelectedDate = selectedDate;
            Assert.Equal(selectedDate, state.SelectedDate);

            indicatorHost.Content = null;
            origin.Content = null;
            floating.Content = view;
            floating.Show();
            Dispatcher.UIThread.RunJobs(DispatcherPriority.Loaded);
            view.Measure(new Size(800, 600));
            view.Arrange(new Rect(0, 0, 800, 600));
            Dispatcher.UIThread.RunJobs();

            var floatingPicker = view.GetVisualDescendants().OfType<DatePicker>().Single();
            Assert.NotSame(originalPicker, floatingPicker);
            Assert.Equal(selectedDate, floatingPicker.SelectedDate);
        }
        finally
        {
            floating.Content = null;
            floating.Close();
            origin.Close();
        }
    }

    private sealed class DatePickerState
    {
        public DateTimeOffset? SelectedDate { get; set; }
    }
}
