using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;

namespace RpcNet.Internal
{
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

        public Span<byte> Read(int length)
        {
            return this.buffer.AsSpan();
        }
    }
}
