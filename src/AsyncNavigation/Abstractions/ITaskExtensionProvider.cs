namespace AsyncNavigation.Abstractions;

public interface ITaskExtensionProvider
{
    public T WaitOnDispatcher<T>(Task<T> task);
    public void WaitOnDispatcher(Task task);
}
