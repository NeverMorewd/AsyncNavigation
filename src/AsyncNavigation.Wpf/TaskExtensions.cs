
namespace System.Windows.Threading;

public static class TaskExtensions
{
    public static T WaitOnDispatcherFrame<T>(this Task<T> task)
    {
        if (!task.IsCompleted)
        {
            var frame = new DispatcherFrame();
            task.ContinueWith(static (_, s) => ((DispatcherFrame)s!).Continue = false, frame);
            Dispatcher.PushFrame(frame);
        }
        return task.GetAwaiter().GetResult();
    }

    public static void WaitOnDispatcherFrame(this Task task)
    {
        if (!task.IsCompleted)
        {
            var frame = new DispatcherFrame();
            task.ContinueWith(static (_, s) => ((DispatcherFrame)s!).Continue = false, frame);
            Dispatcher.PushFrame(frame);
        }
        task.GetAwaiter().GetResult();
    }
}

