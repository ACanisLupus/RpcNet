namespace RpcNet.Internal
{
    using System;
    using System.Net;
    using System.Net.Sockets;

    public class UdpWriter : INetworkWriter
    {
        private readonly byte[] buffer;
        private readonly IPEndPoint remoteIpEndPoint;
        private readonly UdpClient udpClient;

        private int writeIndex;

        public UdpWriter(UdpClient udpClient) : this(udpClient, null, 65536)
        {
        }

        public UdpWriter(UdpClient udpClient, int bufferSize) : this(udpClient, null, bufferSize)
        {
        }

        public UdpWriter(UdpClient udpClient, IPEndPoint remoteIpEndPoint) : this(udpClient, remoteIpEndPoint, 65536)
        {
        }

        public UdpWriter(UdpClient udpClient, IPEndPoint remoteIpEndPoint, int bufferSize)
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
