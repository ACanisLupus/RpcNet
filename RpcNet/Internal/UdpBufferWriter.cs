namespace RpcNet.Internal
{
    using System;
    using System.Net;
    using System.Net.Sockets;
    using System.Threading;

    // Public for tests
    public class UdpBufferWriter : INetworkWriter, IDisposable
    {
        private readonly Socket socket;
        private readonly byte[] buffer;
        private readonly SocketAsyncEventArgs socketAsyncEventArgs = new SocketAsyncEventArgs();
        private readonly ManualResetEvent manualResetEvent = new ManualResetEvent(false);
        private int writeIndex;

        public UdpBufferWriter(Socket socket) : this(socket, 65536)
        {
        }

        public UdpBufferWriter(Socket socket, int bufferSize)
        {
            if (bufferSize < sizeof(int) || bufferSize % 4 != 0)
            {
                throw new ArgumentOutOfRangeException(nameof(bufferSize));
            }

            this.socket = socket;
            this.buffer = new byte[bufferSize];
            this.socketAsyncEventArgs.Completed += this.OnCompleted;
        }

        private void OnCompleted(object sender, SocketAsyncEventArgs e) => this.manualResetEvent.Set();

        public void BeginWriting() => this.writeIndex = 0;

        // Not the best SendTo-implementation, but a simple one, that is SocketException-free
        public UdpResult EndWriting(IPEndPoint remoteEndPoint)
        {
            this.manualResetEvent.Reset();
            this.socketAsyncEventArgs.RemoteEndPoint = remoteEndPoint;
            this.socketAsyncEventArgs.SetBuffer(this.buffer, 0, this.writeIndex);

            bool willRaiseEvent = this.socket.SendToAsync(this.socketAsyncEventArgs);
            if (willRaiseEvent)
            {
                if (!this.manualResetEvent.WaitOne())
                {
                    return new UdpResult { SocketError = SocketError.TimedOut };
                }
            }

            if (this.socketAsyncEventArgs.SocketError != SocketError.Success)
            {
                return new UdpResult { SocketError = this.socketAsyncEventArgs.SocketError };
            }

            return new UdpResult
            {
                BytesLength = this.socketAsyncEventArgs.BytesTransferred,
                IpEndPoint = (IPEndPoint)this.socketAsyncEventArgs.RemoteEndPoint
            };
        }

        public Span<byte> Reserve(int length)
        {
            if (this.writeIndex + length > this.buffer.Length)
            {
                throw new RpcException("Buffer overflow.");
            }

            Span<byte> span = this.buffer.AsSpan(this.writeIndex, length);
            this.writeIndex += length;
            return span;
        }

        public void Dispose()
        {
            this.socketAsyncEventArgs.Dispose();
            this.manualResetEvent.Dispose();
        }
    }
}
