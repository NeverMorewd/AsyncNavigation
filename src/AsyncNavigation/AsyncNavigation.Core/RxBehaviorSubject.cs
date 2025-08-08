/*

The MIT License (MIT)

Copyright (c) .NET Foundation and Contributors

All rights reserved.

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.

*/

using System.Diagnostics.CodeAnalysis;

namespace AsyncNavigation.Core;

/// <summary>
/// https://github.com/dotnet/reactive/blob/de5749f47220b27d9cb1254495246df92de5b8dd/Rx.NET/Source/src/System.Reactive/Subjects/BehaviorSubject.cs
/// Represents a value that changes over time.
/// Observers can subscribe to the subject to receive the last (or initial) value and all subsequent notifications.
/// </summary>
/// <typeparam name="T">The type of the elements processed by the subject.</typeparam>
/// <remarks>
/// Initializes a new instance of the <see cref="RxBehaviorSubject{T}"/> class which creates a subject that caches its last value and starts with the specified value.
/// </remarks>
/// <param name="value">Initial value sent to observers when no other value has been received by the subject yet.</param>
public sealed class RxBehaviorSubject<T>(T value) : IObserver<T>, IObservable<T>, IDisposable
{
    #region Fields

    private readonly object _gate = new();
    private RxImmutableList<IObserver<T>> _observers = RxImmutableList<IObserver<T>>.Empty;
    private bool _isStopped;
    private T _value = value;
    private Exception? _exception;
    private bool _isDisposed;

    #endregion

    #region Properties

    /// <summary>
    /// Indicates whether the subject has observers subscribed to it.
    /// </summary>
    public bool HasObservers => _observers?.Data.Length > 0;

    /// <summary>
    /// Indicates whether the subject has been disposed.
    /// </summary>
    public bool IsDisposed
    {
        get
        {
            lock (_gate)
            {
                return _isDisposed;
            }
        }
    }

    /// <summary>
    /// Gets the current value or throws an exception.
    /// </summary>
    /// <value>The initial value passed to the constructor until <see cref="OnNext"/> is called; after which, the last value passed to <see cref="OnNext"/>.</value>
    /// <remarks>
    /// <para><see cref="Value"/> is frozen after <see cref="OnCompleted"/> is called.</para>
    /// <para>After <see cref="OnError"/> is called, <see cref="Value"/> always throws the specified exception.</para>
    /// <para>An exception is always thrown after <see cref="Dispose"/> is called.</para>
    /// <alert type="caller">
    /// Reading <see cref="Value"/> is a thread-safe operation, though there's a potential race condition when <see cref="OnNext"/> or <see cref="OnError"/> are being invoked concurrently.
    /// In some cases, it may be necessary for a caller to use external synchronization to avoid race conditions.
    /// </alert>
    /// </remarks>
    /// <exception cref="ObjectDisposedException">Dispose was called.</exception>
    public T Value
    {
        get
        {
            lock (_gate)
            {
                CheckDisposed();

                if (_exception is not null)
                {
                    throw _exception;
                }
                return _value;
            }
        }
    }

    #endregion

    #region Methods

    /// <summary>
    /// Tries to get the current value or throws an exception.
    /// </summary>
    /// <param name="value">The initial value passed to the constructor until <see cref="OnNext"/> is called; after which, the last value passed to <see cref="OnNext"/>.</param>
    /// <returns>true if a value is available; false if the subject was disposed.</returns>
    /// <remarks>
    /// <para>The value returned from <see cref="TryGetValue"/> is frozen after <see cref="OnCompleted"/> is called.</para>
    /// <para>After <see cref="OnError"/> is called, <see cref="TryGetValue"/> always throws the specified exception.</para>
    /// <alert type="caller">
    /// Calling <see cref="TryGetValue"/> is a thread-safe operation, though there's a potential race condition when <see cref="OnNext"/> or <see cref="OnError"/> are being invoked concurrently.
    /// In some cases, it may be necessary for a caller to use external synchronization to avoid race conditions.
    /// </alert>
    /// </remarks>
    public bool TryGetValue([MaybeNullWhen(false)] out T value)
    {
        lock (_gate)
        {
            if (_isDisposed)
            {
                value = default;
                return false;
            }

            if (_exception is not null)
            {
                throw _exception;
            }

            value = _value;
            return true;
        }
    }

    #region IObserver<T> implementation

    /// <summary>
    /// Notifies all subscribed observers about the end of the sequence.
    /// </summary>
    public void OnCompleted()
    {
        IObserver<T>[]? os = null;

        lock (_gate)
        {
            CheckDisposed();

            if (!_isStopped)
            {
                os = _observers.Data;
                _observers = RxImmutableList<IObserver<T>>.Empty;
                _isStopped = true;
            }
        }

        if (os != null)
        {
            foreach (var o in os)
            {
                o.OnCompleted();
            }
        }
    }

    /// <summary>
    /// Notifies all subscribed observers about the exception.
    /// </summary>
    /// <param name="error">The exception to send to all observers.</param>
    /// <exception cref="ArgumentNullException"><paramref name="error"/> is <c>null</c>.</exception>
    public void OnError(Exception error)
    {
        ArgumentNullException.ThrowIfNull(error);

        IObserver<T>[]? os = null;

        lock (_gate)
        {
            CheckDisposed();

            if (!_isStopped)
            {
                os = _observers.Data;
                _observers = RxImmutableList<IObserver<T>>.Empty;
                _isStopped = true;
                _exception = error;
            }
        }

        if (os != null)
        {
            foreach (var o in os)
            {
                o.OnError(error);
            }
        }
    }

    /// <summary>
    /// Notifies all subscribed observers about the arrival of the specified element in the sequence.
    /// </summary>
    /// <param name="value">The value to send to all observers.</param>
    public void OnNext(T value)
    {
        IObserver<T>[]? os = null;

        lock (_gate)
        {
            CheckDisposed();

            if (!_isStopped)
            {
                _value = value;
                os = _observers.Data;
            }
        }

        if (os != null)
        {
            foreach (var o in os)
            {
                o.OnNext(value);
            }
        }
    }

    #endregion

    #region IObservable<T> implementation

    /// <summary>
    /// Subscribes an observer to the subject.
    /// </summary>
    /// <param name="observer">Observer to subscribe to the subject.</param>
    /// <returns>Disposable object that can be used to unsubscribe the observer from the subject.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="observer"/> is <c>null</c>.</exception>
    public IDisposable Subscribe(IObserver<T> observer)
    {
        ArgumentNullException.ThrowIfNull(observer);

        Exception? ex;

        lock (_gate)
        {
            CheckDisposed();

            if (!_isStopped)
            {
                _observers = _observers.Add(observer);
                observer.OnNext(_value);
                return new Subscription(this, observer);
            }

            ex = _exception;
        }

        if (ex != null)
        {
            observer.OnError(ex);
        }
        else
        {
            observer.OnCompleted();
        }

        return EmptyDisposable.Instance;
    }

    private void Unsubscribe(IObserver<T> observer)
    {
        lock (_gate)
        {
            if (!_isDisposed)
            {
                _observers = _observers.Remove(observer);
            }
        }
    }

    #endregion

    #region IDisposable implementation

    /// <summary>
    /// Unsubscribe all observers and release resources.
    /// </summary>
    public void Dispose()
    {
        lock (_gate)
        {
            _isDisposed = true;
            _observers = null!; // NB: Disposed checks happen prior to accessing _observers.
            _value = default!;
            _exception = null;
        }
    }

    private void CheckDisposed()
    {
        if (!_isDisposed)
        {
            return;
        }
        throw new ObjectDisposedException(string.Empty);
    }

    #endregion

    private sealed class Subscription(RxBehaviorSubject<T> subject, IObserver<T> observer) : IDisposable
    {
        private RxBehaviorSubject<T> _subject = subject;
        private IObserver<T>? _observer = observer;

        public void Dispose()
        {
            var observer = Interlocked.Exchange(ref _observer, null);
            if (observer == null)
            {
                return;
            }

            _subject.Unsubscribe(observer);
            _subject = null!;
        }
    }

    #endregion
}
