namespace RpcNet.Internal
{
    using System;
    using System.Net;
    using System.Net.Sockets;
    using System.Runtime.InteropServices;

    public class UdpReader : INetworkReader
    {
        private readonly byte[] buffer;
        private readonly Socket udpClient;

        private int readIndex;
        private EndPoint remoteEndPoint = new IPEndPoint(IPAddress.Loopback, 0);
        private int totalLength;

        public UdpReader(Socket udpClient) : this(udpClient, 65536)
        {
        }

        public UdpReader(Socket udpClient, int bufferSize)
        {
            if ((bufferSize < sizeof(int)) || ((bufferSize % 4) != 0))
            {
                throw new ArgumentOutOfRangeException(nameof(bufferSize));
            }

            this.udpClient = udpClient;

            // See
            // https://stackoverflow.com/questions/7201862/an-existing-connection-was-forcibly-closed-by-the-remote-host
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                const uint IOC_IN = 0x80000000;
                const uint IOC_VENDOR = 0x18000000;
                uint SIO_UDP_CONNRESET = IOC_IN | IOC_VENDOR | 12;
                this.udpClient.IOControl((int)SIO_UDP_CONNRESET, new[] { Convert.ToByte(false) }, null);
            }

            this.buffer = new byte[bufferSize];
        }

        public NetworkReadResult BeginReading()
        {
            this.readIndex = 0;
            try
            {
                this.totalLength = this.udpClient.ReceiveFrom(
                    this.buffer,
                    SocketFlags.None,
                    ref this.remoteEndPoint);
                return NetworkReadResult.CreateSuccess((IPEndPoint)this.remoteEndPoint);
            }
            catch (SocketException e)
            {
                return NetworkReadResult.CreateError(e.SocketErrorCode);
            }
        }

        public void EndReading()
        {
            if (this.readIndex != this.totalLength)
            {
                const string ErrorMessage = "Not all data was read.";
                throw new RpcException(ErrorMessage);
            }
        }

        public ReadOnlySpan<byte> Read(int length)
        {
            if ((this.readIndex + length) > this.totalLength)
            {
                const string ErrorMessage = "Buffer underflow.";
                throw new RpcException(ErrorMessage);
            }

            Span<byte> span = this.buffer.AsSpan(this.readIndex, length);
            this.readIndex += length;
            return span;
        }
    }
}
