using System.Threading.Channels;

namespace AsyncNavigation;

public class AsyncConcurrentItem<T> : IDisposable
{
    private readonly Channel<T> _channel;
    private readonly ChannelWriter<T> _writer;
    private readonly ChannelReader<T> _reader;
    private volatile bool _disposed;

    public AsyncConcurrentItem()
    {
        var options = new BoundedChannelOptions(1)
        {
            FullMode = BoundedChannelFullMode.DropOldest,
            SingleReader = false,
            SingleWriter = false,
            AllowSynchronousContinuations = true
        };

        _channel = Channel.CreateBounded<T>(options);
        _writer = _channel.Writer;
        _reader = _channel.Reader;
    }

    public void SetData(T data)
    {
        ThrowIfDisposed();

        if (!_writer.TryWrite(data))
        {
            throw new InvalidOperationException("Unable to write data to channel");
        }
    }

    public async ValueTask SetDataAsync(T data, CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();
        await _writer.WriteAsync(data, cancellationToken).ConfigureAwait(false);
    }

    public T TakeData()
    {
        ThrowIfDisposed();
        if (_reader.TryRead(out var data))
        {
            return data;
        }
        return _reader.ReadAsync().AsTask().GetAwaiter().GetResult();
    }
    public async ValueTask<T> TakeDataAsync(CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();
        return await _reader.ReadAsync(cancellationToken).ConfigureAwait(false);
    }
    public bool TryTakeData(out T? data)
    {
        ThrowIfDisposed();
        return _reader.TryRead(out data);
    }
    public bool WaitForData(TimeSpan timeout, out T? data)
    {
        ThrowIfDisposed();

        using var cts = new CancellationTokenSource(timeout);
        try
        {
            data = _reader.ReadAsync(cts.Token).AsTask().GetAwaiter().GetResult();
            return true;
        }
        catch (OperationCanceledException)
        {
            data = default;
            return false;
        }
    }
    public async ValueTask<T?> WaitForDataAsync(TimeSpan timeout, CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();

        using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        cts.CancelAfter(timeout);

        try
        {
            return await _reader.ReadAsync(cts.Token).ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (cts.IsCancellationRequested && !cancellationToken.IsCancellationRequested)
        {
            return default;
        }
    }
    public IAsyncEnumerable<T> ReadAllAsync(CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();
        return _reader.ReadAllAsync(cancellationToken);
    }
    public ValueTask<bool> WaitToReadAsync(CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();
        return _reader.WaitToReadAsync(cancellationToken);
    }
    public bool HasData => _reader.TryPeek(out _);

    public void CompleteWriter()
    {
        if (!_disposed)
        {
            _writer.Complete();
        }
    }
    public void CompleteWriter(Exception? error)
    {
        if (!_disposed)
        {
            _writer.Complete(error);
        }
    }

    private void ThrowIfDisposed()
    {
        if (!_disposed)
        {
            return;
        }
        throw new ObjectDisposedException(nameof(AsyncConcurrentItem<T>));
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            _disposed = true;
            _writer.Complete();
            GC.SuppressFinalize(this);
        }
    }
}