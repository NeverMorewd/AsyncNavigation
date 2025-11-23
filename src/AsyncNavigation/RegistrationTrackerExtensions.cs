using AsyncNavigation.Abstractions;
using AsyncNavigation.Core;
using System.Diagnostics.CodeAnalysis;

namespace AsyncNavigation;

public static class RegistrationTrackerExtensions
{
    public static bool TryGetAll(this IRegistrationTracker registrationTracker,
        RegistryCategory category,
        [MaybeNullWhen(false)] out IReadOnlyCollection<string> names)
    {
        return registrationTracker.GetAll().TryGetValue(category, out names);
    }

    public static bool TryGetViews(this IRegistrationTracker registrationTracker,
        [MaybeNullWhen(false)] out IReadOnlyCollection<string> names)
    {
        return registrationTracker.TryGetAll(RegistryCategory.View, out names);
    }
    public static bool TryGetNavigations(this IRegistrationTracker registrationTracker,
        [MaybeNullWhen(false)] out IReadOnlyCollection<string> names)
    {
        return registrationTracker.TryGetAll(RegistryCategory.Navigation, out names);
    }
    public static bool TryGetDialogs(this IRegistrationTracker registrationTracker,
        [MaybeNullWhen(false)] out IReadOnlyCollection<string> names)
    {
        return registrationTracker.TryGetAll(RegistryCategory.Dialog, out names);
    }

    internal static void TrackView(this IRegistrationTracker registrationTracker, string name)
    {
        registrationTracker.Register(RegistryCategory.View, name);
        registrationTracker.Register(RegistryCategory.Navigation, name);
        registrationTracker.Register(RegistryCategory.Dialog, name);
    }
    internal static void TrackNavigation(this IRegistrationTracker registrationTracker, string name)
    {
        registrationTracker.Register(RegistryCategory.Navigation, name);
    }
    internal static void TrackDialog(this IRegistrationTracker registrationTracker, string name)
    {
        registrationTracker.Register(RegistryCategory.Dialog, name);
    }
}