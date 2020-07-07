// --------------------------------------------------------------------------------------------------------------------
// <copyright company="dSPACE GmbH" file="UdpWriter.cs">
//   Copyright dSPACE GmbH. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace RpcNet.Internal
{
    using System;
    using System.Net;
    using System.Net.Sockets;

    // This service writer for UDP is asynchronous, because there is
    // no synchronous method to call SendTo without a SocketException.
    // SocketExceptions on server side are ugly!
    // Making SendToAsync synchronous using a reset event would block two threads instead of one,
    // therefore the implementation is fully asynchronous (not async/await).
    public class UdpWriter : INetworkWriter, IDisposable
    {
        private readonly byte[] buffer;
        private readonly ILogger logger;
        private readonly IPEndPoint remoteIpEndPoint;
        private readonly UdpClient udpClient;
        private readonly SocketAsyncEventArgs socketAsyncEventArgs = new SocketAsyncEventArgs();

        private int writeIndex;

        public UdpWriter(UdpClient udpClient, ILogger logger) : this(udpClient, null, 65536, logger)
        {
        }

        public UdpWriter(UdpClient udpClient, int bufferSize, ILogger logger) : this(udpClient, null, bufferSize, logger)
        {
        }

        public UdpWriter(UdpClient udpClient, IPEndPoint remoteIpEndPoint, ILogger logger) : this(
            udpClient,
            remoteIpEndPoint,
            65536,
            logger)
        {
        }

        public UdpWriter(UdpClient udpClient, IPEndPoint remoteIpEndPoint, int bufferSize, ILogger logger)
        {
            if (bufferSize < sizeof(int) || bufferSize % 4 != 0)
            {
                throw new ArgumentOutOfRangeException(nameof(bufferSize));
            }

            this.udpClient = udpClient;
            this.remoteIpEndPoint = remoteIpEndPoint;
            this.buffer = new byte[bufferSize];
            this.socketAsyncEventArgs.Completed += this.OnCompleted;
            this.logger = logger;
        }

        public Action<NetworkWriteResult> Completed { get; set; }

        public void BeginWriting() => this.writeIndex = 0;

        public void EndWritingAsync(IPEndPoint remoteEndPoint)
        {
            this.socketAsyncEventArgs.RemoteEndPoint = remoteEndPoint;
            this.socketAsyncEventArgs.SetBuffer(this.buffer, 0, this.writeIndex);

            bool willRaiseEvent = this.udpClient.Client.SendToAsync(this.socketAsyncEventArgs);
            if (!willRaiseEvent)
            {
                this.OnCompleted(this, this.socketAsyncEventArgs);
            }
        }

        public NetworkWriteResult EndWriting() => this.EndWriting(this.remoteIpEndPoint);

        public NetworkWriteResult EndWriting(IPEndPoint remoteEndPoint)
        {
            try
            {
                this.udpClient.Client.SendTo(this.buffer, this.writeIndex, SocketFlags.None, remoteEndPoint);
                return new NetworkWriteResult(SocketError.Success);
            }
            catch (SocketException exception)
            {
                return new NetworkWriteResult(exception.SocketErrorCode);
            }
        }

        public Span<byte> Reserve(int length)
        {
            if (this.writeIndex + length > this.buffer.Length)
            {
                const string ErrorMessage = "Buffer overflow.";
                this.logger?.Error(ErrorMessage);
                throw new RpcException(ErrorMessage);
            }

            Span<byte> span = this.buffer.AsSpan(this.writeIndex, length);
            this.writeIndex += length;
            return span;
        }

        public void Dispose() => this.socketAsyncEventArgs.Dispose();

        private void OnCompleted(object sender, SocketAsyncEventArgs e) =>
            this.Completed?.Invoke(new NetworkWriteResult(e.SocketError));
    }
}
