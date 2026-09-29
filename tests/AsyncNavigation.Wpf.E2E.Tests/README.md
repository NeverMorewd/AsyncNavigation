# WPF floating reference test

This Windows-only test documents how WPF behaves when the same view is moved
between two top-level windows. It intentionally lives outside
`src/AsyncNavigation.slnx`, so normal solution builds and CI test runs do not
execute it.

Run it explicitly on an interactive Windows machine:

```powershell
dotnet test tests/AsyncNavigation.Wpf.E2E.Tests/AsyncNavigation.Wpf.E2E.Tests.csproj
```

The tests do not use UI Automation. They run WPF directly on an STA thread and
check the generated `DatePicker` instance and its date before floating, while
floating, and after restoration. Both unbound UI state and TwoWay-bound MVVM
state are covered.

The lifecycle regression tests verify that the Dock to region button requests
restoration, while the normal window close button requests closure without
restoring content. Closure is deferred to avoid re-entering `Window.Close()`
inside WPF's synchronous `Closing` event.

The navigation integration tests exercise both ContentRegion and TabRegion with
real views and windows. They verify that navigating to a cached floating instance
activates its existing window, while a new instance can open in the main region.
Docking into an occupied content region switches to the original instance and
keeps the previous view available through Back. Docking into a tab region keeps
other tabs and selects the restored tab. Equivalent tests run in the Avalonia
headless suite with Fluent templates.
