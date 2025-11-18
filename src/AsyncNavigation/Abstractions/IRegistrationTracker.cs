using AsyncNavigation.Core;

namespace AsyncNavigation.Abstractions;

public interface IRegistrationTracker
{
    void Register(RegistryCategory category, string key);
    IReadOnlyDictionary<RegistryCategory, IReadOnlyCollection<string>> GetAll();
}