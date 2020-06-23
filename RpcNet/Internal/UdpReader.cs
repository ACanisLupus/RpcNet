namespace RpcNet.Internal
{
    using System;
    using System.Net;
    using System.Net.Sockets;

    // This service reader for UDP is asynchronous, because there is
    // no synchronous method to call ReceiveFrom without a SocketException.
    // SocketExceptions on server side are ugly!
    // Making ReceiveFromAsync synchronous using a reset event would block two threads instead of one,
    // therefore the implementation is fully asynchronous (not async/await).
    public class UdpReader : INetworkReader, IDisposable
    {
        private readonly byte[] buffer;
        private readonly Socket socket;
        private readonly SocketAsyncEventArgs socketAsyncEventArgs = new SocketAsyncEventArgs();

        private int readIndex;
        private int totalLength;

        public UdpReader(Socket socket) : this(socket, 65536)
        {
        }

        public UdpReader(Socket socket, int bufferSize)
        {
            if (bufferSize < sizeof(int) || bufferSize % 4 != 0)
            {
                throw new ArgumentOutOfRangeException(nameof(bufferSize));
            }

            this.socket = socket;
            this.buffer = new byte[bufferSize];
            this.socketAsyncEventArgs.Completed += this.OnCompleted;
            this.socketAsyncEventArgs.SetBuffer(this.buffer, 0, bufferSize);
            this.socketAsyncEventArgs.RemoteEndPoint = new IPEndPoint(IPAddress.Loopback, 0);
        }

        public Action<NetworkResult> Completed { get; set; }

        public NetworkResult BeginReading()
        {
            this.readIndex = 0;
            try
            {
                EndPoint endPoint = new IPEndPoint(IPAddress.Loopback, 0);
                this.totalLength = this.socket.ReceiveFrom(this.buffer, ref endPoint);
                return new NetworkResult { RemoteIpEndPoint = (IPEndPoint)endPoint };
            }
            catch (SocketException exception)
            {
                return new NetworkResult { SocketError = exception.SocketErrorCode };
            }
        }

        public void BeginReadingAsync()
        {
            this.readIndex = 0;
            bool willRaiseEvent = this.socket.ReceiveFromAsync(this.socketAsyncEventArgs);
            if (!willRaiseEvent)
            {
                this.OnCompleted(this, this.socketAsyncEventArgs);
            }
        }

        public void EndReading()
        {
            if (this.readIndex != this.totalLength)
            {
                throw new RpcException("Not all data was read.");
            }
        }

        public void Dispose() => this.socketAsyncEventArgs.Dispose();

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

        private void OnCompleted(object sender, SocketAsyncEventArgs e)
        {
            if (this.socketAsyncEventArgs.SocketError != SocketError.Success)
            {
                this.Completed?.Invoke(new NetworkResult { SocketError = this.socketAsyncEventArgs.SocketError });
            }

            this.totalLength = this.socketAsyncEventArgs.BytesTransferred;
            this.Completed?.Invoke(
                new NetworkResult { RemoteIpEndPoint = (IPEndPoint)this.socketAsyncEventArgs.RemoteEndPoint });
        }
    }
}
