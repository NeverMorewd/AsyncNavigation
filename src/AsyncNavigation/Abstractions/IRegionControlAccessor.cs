using System.Diagnostics.CodeAnalysis;

namespace AsyncNavigation.Abstractions;

public interface IRegionControlAccessor<TControl> where TControl : class
{
    void ExecuteOn(Action<TControl> action);
    TResult ExecuteOn<TResult>(Func<TControl, TResult> func);
    TControl Ensure();
    bool TryGet([MaybeNullWhen(false)] out TControl control);
}
