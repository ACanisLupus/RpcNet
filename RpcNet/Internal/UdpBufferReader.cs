namespace RpcNet.Internal
{
    using System;
    using System.Net;
    using System.Net.Sockets;
    using System.Threading;

    public class UdpBufferReader : INetworkReader, IDisposable
    {
        private readonly Socket socket;
        private readonly byte[] buffer;
        private readonly SocketAsyncEventArgs socketAsyncEventArgs = new SocketAsyncEventArgs();
        private readonly ManualResetEvent manualResetEvent = new ManualResetEvent(false);
        private int totalLength;
        private int readIndex;

        public UdpBufferReader(Socket socket) : this(socket, 65536)
        {
        }

        public UdpBufferReader(Socket socket, int bufferSize)
        {
            this.socket = socket;
            this.buffer = new byte[bufferSize];
            this.socketAsyncEventArgs.Completed += this.OnCompleted;
            this.socketAsyncEventArgs.SetBuffer(this.buffer, 0, bufferSize);
            this.socketAsyncEventArgs.RemoteEndPoint = new IPEndPoint(IPAddress.Loopback, 0);
        }

        private void OnCompleted(object sender, SocketAsyncEventArgs e) => this.manualResetEvent.Set();

        // Not the best ReceiveFrom-implementation, but a simple one, that is SocketException-free
        public UdpResult BeginReading(int timeout = -1)
        {
            this.manualResetEvent.Reset();
            if (this.socket.ReceiveFromAsync(this.socketAsyncEventArgs))
            {
                if (!this.manualResetEvent.WaitOne(timeout))
                {
                    return new UdpResult { SocketError = SocketError.TimedOut };
                }
            }

            if (this.socketAsyncEventArgs.SocketError != SocketError.Success)
            {
                return new UdpResult { SocketError = this.socketAsyncEventArgs.SocketError };
            }

            this.totalLength = this.socketAsyncEventArgs.BytesTransferred;
            return new UdpResult
            {
                BytesLength = this.totalLength,
                IpEndPoint = (IPEndPoint)this.socketAsyncEventArgs.RemoteEndPoint
            };
        }

        public void Dispose()
        {
            this.socketAsyncEventArgs.Dispose();
        }

        public ReadOnlySpan<byte> Read(int length)
        {
            if (this.readIndex + length > this.totalLength)
            {
                throw new RpcException("Buffer underflow.");
            }

            Span<byte> span = this.buffer.AsSpan(this.readIndex, length);
            this.readIndex += length;
            return span;
        }
    }
}
