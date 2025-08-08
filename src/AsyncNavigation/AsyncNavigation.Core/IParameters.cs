using System.Diagnostics.CodeAnalysis;

namespace AsyncNavigation.Core;

public interface IParameters : IEnumerable<KeyValuePair<string, object>>
{
    object? this[string key] { get; }
    void Add(string key, object value);
    void AddRange(IEnumerable<KeyValuePair<string, object>> entries);
    bool ContainsKey(string key);
    int Count { get; }
    IEnumerable<string> Keys { get; }
    T GetValue<T>(string key) where T : notnull;
    IEnumerable<T> GetValues<T>(string key) where T : notnull;
    bool TryGetValue<T>(string key, [MaybeNullWhen(false)] out T value) where T : notnull;
}
