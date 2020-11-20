namespace Test
{
    using System;
    using System.Collections.Concurrent;
    using System.Threading;

    internal class Channel<T>
    {
        private readonly AutoResetEvent itemReceived = new AutoResetEvent(false);
        private readonly ConcurrentQueue<T> items = new ConcurrentQueue<T>();

        public void Send(T item)
        {
            this.items.Enqueue(item);
            this.itemReceived.Set();
        }

        public bool TryReceive(TimeSpan timeout, out T item)
        {
            item = default;

            return this.itemReceived.WaitOne(timeout) && this.items.TryDequeue(out item);
        }
    }
}
