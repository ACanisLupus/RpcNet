namespace RpcNet.Internal
{
    using System;
    using System.IO;

    public class TcpBufferWriter : INetworkWriter
    {
        private const int TcpHeader = 4;
        private readonly Stream stream;
        private readonly byte[] buffer = new byte[65536];
        private int index;

        public TcpBufferWriter(Stream stream) => this.stream = stream;

        public void Reset() => this.index = TcpHeader;
        public void EndWriting() => this.FlushPacket(true);

        public Span<byte> Reserve(int length)
        {
            // For now there is no way to split extremely large opaque datas or strings into multiple packets
            if (length + TcpHeader > this.buffer.Length)
            {
                throw new RpcException($"Buffer overflow. Could not reserve more than {this.buffer.Length - TcpHeader} bytes.");
            }

            if (this.index + length > this.buffer.Length)
            {
                this.FlushPacket(false);
            }

            Span<byte> span = this.buffer.AsSpan(this.index, length);
            this.index += length;
            return span;
        }

        // TODO: Flush must throw an RpcException
        private void FlushPacket(bool lastPacket)
        {
            // Last packet sets first bit to 1
            int length = lastPacket ? (int)((this.index - TcpHeader) & 0x80000000) : (this.index - TcpHeader);
            Utilities.WriteBytesBigEndian(this.buffer.AsSpan(), length);
            this.stream.Write(this.buffer, 0, length + TcpHeader);
            this.Reset();
        }
    }
}
