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

The lifecycle regression test also closes a floating window through its normal
close button path and verifies that its content is restored and the now-empty
window actually closes. This protects against re-entering `Window.Close()` from
inside WPF's synchronous `Closing` event.
