// Copyright by Artur Wolf

namespace Test;

using System.Collections.Concurrent;

internal class Channel<T>
{
    private readonly AutoResetEvent _itemReceived = new(false);
    private readonly ConcurrentQueue<T> _items = new();

    public void Send(T item)
    {
        _items.Enqueue(item);
        _itemReceived.Set();
    }

    public bool TryReceive(TimeSpan timeout, out T item)
    {
        item = default;
        return _itemReceived.WaitOne(timeout) && _items.TryDequeue(out item);
    }
}
