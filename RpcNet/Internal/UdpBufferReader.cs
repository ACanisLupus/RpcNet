namespace RpcNet.Internal
{
    using System;
    using System.IO;

    // TODO: Must read from socket, otherwise the remote ip end point is unknown
    public class UdpBufferReader : INetworkReader
    {
        private readonly Stream stream;
        private readonly byte[] buffer = new byte[65536];
        private int totalLength;
        private int index;

        public UdpBufferReader(Stream stream) => this.stream = stream;

        public void BeginReading()
        {
            this.totalLength = this.stream.Read(this.buffer, 0, this.buffer.Length);
            this.index = 0;
        }

        public void EndReading()
        {

        }

        public Span<byte> Read(int length)
        {
            if (this.index + length > this.totalLength)
            {
                throw new RpcException("Buffer underflow.");
            }

            Span<byte> span = this.buffer.AsSpan(this.index, length);
            this.index += length;
            return span;
        }
    }
}
