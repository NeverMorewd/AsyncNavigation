namespace AsyncNavigation.Core;

public delegate Task AsyncEventHandler<in TEventArgs>(object sender, TEventArgs e) where TEventArgs : EventArgs;
