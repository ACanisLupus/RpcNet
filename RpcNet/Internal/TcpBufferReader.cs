namespace RpcNet.Internal
{
    using System;
    using System.Net.Sockets;

    // TODO: Implement
    public class TcpBufferReader : INetworkReader
    {
        private byte[] buffer = new byte[65536];

        public SocketError BeginReading()
        {
            return SocketError.Success;
        }

        public void EndReading()
        {
        }

        public ReadOnlySpan<byte> Read(int length)
        {
            return this.buffer.AsSpan();
        }
    }
}
