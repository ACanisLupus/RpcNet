namespace RpcNet.Internal
{
    using System;
    using System.Net;
    using System.Net.Sockets;

    public class UdpBufferReader : INetworkReader
    {
        private readonly Socket socket;
        private readonly byte[] buffer;
        private int totalLength;
        private int readIndex;

        public UdpBufferReader(Socket socket) : this(socket, 65536)
        {
        }

        public UdpBufferReader(Socket socket, int bufferSize)
        {
            this.socket = socket;
            this.buffer = new byte[bufferSize];
        }

        public void BeginReading(out IPEndPoint remoteEndPoint)
        {
            EndPoint endPoint = new IPEndPoint(IPAddress.Loopback, 0);
            this.totalLength = this.socket.ReceiveFrom(this.buffer, ref endPoint);
            remoteEndPoint = (IPEndPoint)endPoint;
        }

        public Span<byte> Read(int length)
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
