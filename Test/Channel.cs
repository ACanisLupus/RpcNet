namespace RpcNet.Test
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;

    public class Channel<T>
    {
        private BlockingCollection<T> _buffer;

        public Channel() : this(1) { }
        public Channel(int size)
        {
            _buffer = new BlockingCollection<T>(new ConcurrentQueue<T>(), size);
        }

        public bool Send(T t)
        {
            try
            {
                _buffer.Add(t);
            }
            catch (InvalidOperationException)
            {
                // will be thrown when the collection gets closed
                return false;
            }

            return true;
        }

        public bool Receive(out T val)
        {
            try
            {
                val = _buffer.Take();
            }
            catch (InvalidOperationException)
            {
                // will be thrown when the collection is empty and got closed
                val = default;
                return false;
            }

            return true;
        }

        public void Close()
        {
            _buffer.CompleteAdding();
        }

        public IEnumerable<T> Range()
        {
            while (this.Receive(out T val))
            {
                yield return val;
            }
        }
    }
}
