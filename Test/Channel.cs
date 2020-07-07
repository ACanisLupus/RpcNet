namespace RpcNet.Test
{
    using System;
    using System.Collections.Concurrent;

    public class Channel<T>
    {
        private readonly BlockingCollection<T> buffer;

        public Channel() : this(1)
        {
        }

        public Channel(int size)
        {
            this.buffer = new BlockingCollection<T>(new ConcurrentQueue<T>(), size);
        }

        public bool Send(T t)
        {
            try
            {
                this.buffer.Add(t);
            }
            catch (InvalidOperationException)
            {
                // Will be thrown when the collection gets closed
                return false;
            }

            return true;
        }

        public bool Receive(out T val)
        {
            try
            {
                val = this.buffer.Take();
            }
            catch (InvalidOperationException)
            {
                // Will be thrown when the collection is empty and got closed
                val = default;
                return false;
            }

            return true;
        }

        public void Close() => this.buffer.CompleteAdding();
    }
}
