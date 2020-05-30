namespace RpcNet.Internal
{
    using System;
    using System.Net;
    using System.Net.Sockets;

    public class UdpBufferWriter : INetworkWriter
    {
        private readonly Socket socket;
        private readonly byte[] buffer = new byte[65536];
        private int writeIndex;

        public UdpBufferWriter(Socket socket) : this(socket, 65536)
        {
        }

        public UdpBufferWriter(Socket socket, int bufferSize)
        {
            this.socket = socket;
            this.buffer = new byte[bufferSize];
        }

        public void BeginWriting() => this.writeIndex = 0;

        public SocketError EndWriting(IPEndPoint remoteEndPoint, out int bytesSent) =>
            this.socket.SendTo(this.buffer.AsSpan(0, this.writeIndex), remoteEndPoint, out bytesSent);

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
    }
}
