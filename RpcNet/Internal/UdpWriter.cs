namespace RpcNet.Internal
{
    using System;
    using System.Net;
    using System.Net.Sockets;

    public class UdpWriter : INetworkWriter
    {
        private readonly byte[] buffer;
        private readonly IPEndPoint remoteIpEndPoint;
        private readonly Socket udpClient;

        private int writeIndex;

        public UdpWriter(Socket udpClient) : this(udpClient, null, 65536)
        {
        }

        public UdpWriter(Socket udpClient, int bufferSize) : this(udpClient, null, bufferSize)
        {
        }

        public UdpWriter(Socket udpClient, IPEndPoint remoteIpEndPoint) : this(udpClient, remoteIpEndPoint, 65536)
        {
        }

        public UdpWriter(Socket udpClient, IPEndPoint remoteIpEndPoint, int bufferSize)
        {
            if ((bufferSize < sizeof(int)) || ((bufferSize % 4) != 0))
            {
                throw new ArgumentOutOfRangeException(nameof(bufferSize));
            }

            this.udpClient = udpClient;
            this.remoteIpEndPoint = remoteIpEndPoint;
            this.buffer = new byte[bufferSize];
        }

        public void BeginWriting()
        {
            this.writeIndex = 0;
        }

        public NetworkWriteResult EndWriting() => this.EndWriting(this.remoteIpEndPoint);

        public NetworkWriteResult EndWriting(IPEndPoint remoteEndPoint)
        {
            try
            {
                this.udpClient.SendTo(this.buffer, this.writeIndex, SocketFlags.None, remoteEndPoint);
                return new NetworkWriteResult(SocketError.Success);
            }
            catch (SocketException e)
            {
                return new NetworkWriteResult(e.SocketErrorCode);
            }
        }

        public Span<byte> Reserve(int length)
        {
            if ((this.writeIndex + length) > this.buffer.Length)
            {
                const string ErrorMessage = "Buffer overflow.";
                throw new RpcException(ErrorMessage);
            }

            Span<byte> span = this.buffer.AsSpan(this.writeIndex, length);
            this.writeIndex += length;
            return span;
        }
    }
}
