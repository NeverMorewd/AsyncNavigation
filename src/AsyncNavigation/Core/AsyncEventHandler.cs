namespace AsyncNavigation.Core;

public delegate Task AsyncEventHandler<TEventArgs>(object sender, TEventArgs e) where TEventArgs : EventArgs;
