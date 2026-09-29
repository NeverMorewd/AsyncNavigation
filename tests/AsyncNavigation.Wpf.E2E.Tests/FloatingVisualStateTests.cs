using System.ComponentModel;
using System.Runtime.ExceptionServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Threading;

namespace AsyncNavigation.Wpf.E2E.Tests;

public sealed class FloatingVisualStateTests
{
    [Fact]
    public void Unbound_date_survives_moving_the_same_view_between_windows()
    {
        RunOnStaThread(() =>
        {
            var selectedDate = new DateTime(2026, 9, 1);
            var view = CreateTemplatedView(new object());

            using var mainWindow = new TestWindow();
            using var floatingWindow = new TestWindow();

            mainWindow.Content = view;
            mainWindow.Show();
            FlushDispatcher();

            var originalDatePicker = FindVisualChild<DatePicker>(view)
                ?? throw new InvalidOperationException("The DatePicker was not created.");

            originalDatePicker.SelectedDate = selectedDate;
            MoveToFloatingWindowAndBack(
                view,
                originalDatePicker,
                selectedDate,
                mainWindow,
                floatingWindow);
        });
    }

    [Fact]
    public void Bound_date_survives_moving_the_same_view_between_windows()
    {
        RunOnStaThread(() =>
        {
            var selectedDate = new DateTime(2026, 9, 1);
            var state = new ViewState();
            var view = CreateTemplatedView(state, nameof(ViewState.SelectedDate));

            using var mainWindow = new TestWindow();
            using var floatingWindow = new TestWindow();

            mainWindow.Content = view;
            mainWindow.Show();
            FlushDispatcher();

            var originalDatePicker = FindVisualChild<DatePicker>(view)
                ?? throw new InvalidOperationException("The DatePicker was not created.");

            originalDatePicker.SelectedDate = selectedDate;
            FlushDispatcher();
            Assert.Equal(selectedDate, state.SelectedDate);

            MoveToFloatingWindowAndBack(
                view,
                originalDatePicker,
                selectedDate,
                mainWindow,
                floatingWindow);
            Assert.Equal(selectedDate, state.SelectedDate);
        });
    }

    private static void MoveToFloatingWindowAndBack(
        UserControl view,
        DatePicker originalDatePicker,
        DateTime selectedDate,
        TestWindow mainWindow,
        TestWindow floatingWindow)
    {
        mainWindow.Content = null;
        floatingWindow.Content = view;
        floatingWindow.Show();
        FlushDispatcher();

        var floatingDatePicker = FindVisualChild<DatePicker>(view)
            ?? throw new InvalidOperationException("The DatePicker was not found in the floating window.");
        Assert.Same(originalDatePicker, floatingDatePicker);
        Assert.Equal(selectedDate, floatingDatePicker.SelectedDate);

        floatingWindow.Content = null;
        mainWindow.Content = view;
        FlushDispatcher();

        var restoredDatePicker = FindVisualChild<DatePicker>(view)
            ?? throw new InvalidOperationException("The DatePicker was not found after restoration.");
        Assert.Same(originalDatePicker, restoredDatePicker);
        Assert.Equal(selectedDate, restoredDatePicker.SelectedDate);
    }

    private static UserControl CreateTemplatedView(object state, string? selectedDatePath = null)
    {
        var datePickerFactory = new FrameworkElementFactory(typeof(DatePicker));
        if (selectedDatePath is not null)
        {
            datePickerFactory.SetBinding(
                DatePicker.SelectedDateProperty,
                new Binding(selectedDatePath)
                {
                    Mode = BindingMode.TwoWay,
                    UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
                });
        }

        var presenter = new ContentPresenter
        {
            Content = state,
            ContentTemplate = new DataTemplate { VisualTree = datePickerFactory }
        };

        return new UserControl { Content = presenter };
    }

    private static T? FindVisualChild<T>(DependencyObject parent)
        where T : DependencyObject
    {
        for (var i = 0; i < System.Windows.Media.VisualTreeHelper.GetChildrenCount(parent); i++)
        {
            var child = System.Windows.Media.VisualTreeHelper.GetChild(parent, i);
            if (child is T match)
            {
                return match;
            }

            var descendant = FindVisualChild<T>(child);
            if (descendant is not null)
            {
                return descendant;
            }
        }

        return null;
    }

    private static void FlushDispatcher() =>
        Dispatcher.CurrentDispatcher.Invoke(static () => { }, DispatcherPriority.ApplicationIdle);

    private static void RunOnStaThread(Action action)
    {
        Exception? failure = null;
        var thread = new Thread(() =>
        {
            try
            {
                action();
            }
            catch (Exception exception)
            {
                failure = exception;
            }
            finally
            {
                Dispatcher.CurrentDispatcher.InvokeShutdown();
            }
        });

        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        thread.Join();

        if (failure is not null)
        {
            ExceptionDispatchInfo.Capture(failure).Throw();
        }
    }

    private sealed class TestWindow : Window, IDisposable
    {
        public void Dispose() => Close();
    }

    private sealed class ViewState : INotifyPropertyChanged
    {
        private DateTime? _selectedDate;

        public event PropertyChangedEventHandler? PropertyChanged;

        public DateTime? SelectedDate
        {
            get => _selectedDate;
            set
            {
                if (_selectedDate == value)
                {
                    return;
                }

                _selectedDate = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SelectedDate)));
            }
        }
    }
}
