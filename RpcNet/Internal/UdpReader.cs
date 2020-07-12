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
        private readonly ILogger logger;
        private readonly SocketAsyncEventArgs socketAsyncEventArgs = new SocketAsyncEventArgs();
        private readonly UdpClient udpClient;

        private int readIndex;
        private int totalLength;

        public UdpReader(UdpClient udpClient, ILogger logger) : this(udpClient, 65536, logger)
        {
        }

        public UdpReader(UdpClient udpClient, int bufferSize, ILogger logger)
        {
            if ((bufferSize < sizeof(int)) || ((bufferSize % 4) != 0))
            {
                throw new ArgumentOutOfRangeException(nameof(bufferSize));
            }

            this.udpClient = udpClient;
            this.buffer = new byte[bufferSize];
            this.socketAsyncEventArgs.Completed += this.OnCompleted;
            this.socketAsyncEventArgs.SetBuffer(this.buffer, 0, bufferSize);
            this.socketAsyncEventArgs.RemoteEndPoint = new IPEndPoint(IPAddress.Loopback, 0);
            this.logger = logger;
        }

        public Action<NetworkReadResult> Completed { get; set; }

        public NetworkReadResult BeginReading()
        {
            this.readIndex = 0;
            try
            {
                EndPoint endPoint = new IPEndPoint(IPAddress.Loopback, 0);
                this.totalLength = this.udpClient.Client.ReceiveFrom(this.buffer, ref endPoint);
                return NetworkReadResult.CreateSuccess((IPEndPoint)endPoint);
            }
            catch (SocketException exception)
            {
                return NetworkReadResult.CreateError(exception.SocketErrorCode);
            }
        }

        public void BeginReadingAsync()
        {
            this.readIndex = 0;
            bool willRaiseEvent = this.udpClient.Client.ReceiveFromAsync(this.socketAsyncEventArgs);
            if (!willRaiseEvent)
            {
                this.OnCompleted(this, this.socketAsyncEventArgs);
            }
        }

        public void EndReading()
        {
            if (this.readIndex != this.totalLength)
            {
                const string ErrorMessage = "Not all data was read.";
                this.logger?.Error(ErrorMessage);
                throw new RpcException(ErrorMessage);
            }
        }

        public void Dispose()
        {
            this.socketAsyncEventArgs.Dispose();
        }

        public ReadOnlySpan<byte> Read(int length)
        {
            if ((this.readIndex + length) > this.totalLength)
            {
                const string ErrorMessage = "Buffer underflow.";
                this.logger?.Error(ErrorMessage);
                throw new RpcException(ErrorMessage);
            }

            Span<byte> span = this.buffer.AsSpan(this.readIndex, length);
            this.readIndex += length;
            return span;
        }

        private void OnCompleted(object sender, SocketAsyncEventArgs e)
        {
            if (this.socketAsyncEventArgs.SocketError != SocketError.Success)
            {
                this.Completed?.Invoke(NetworkReadResult.CreateError(e.SocketError));
            }

            this.totalLength = this.socketAsyncEventArgs.BytesTransferred;
            this.Completed?.Invoke(
                NetworkReadResult.CreateSuccess((IPEndPoint)this.socketAsyncEventArgs.RemoteEndPoint));
        }
    }
}
