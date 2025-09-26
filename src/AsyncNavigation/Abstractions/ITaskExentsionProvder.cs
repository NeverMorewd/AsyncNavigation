namespace AsyncNavigation.Abstractions;

public interface ITaskExentsionProvder
{
    public T WaitOnDispatcher<T>(Task<T> task);
    public void WaitOnDispatcher(Task task);
}
