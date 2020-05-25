namespace RpcNet.Internal
{
    using System;
    using System.IO;

    public class UdpBufferWriter : IBufferWriter
    {
        private readonly Stream stream;
        private readonly byte[] buffer = new byte[65536];
        private int index;

        public UdpBufferWriter(Stream stream) => this.stream = stream;

        public void BeginWriting() => this.index = 0;

        // TODO: EndWriting must throw an RpcException
        public void EndWriting() => this.stream.Write(this.buffer, 0, this.index);

        public Span<byte> Reserve(int length)
        {
            if (this.index + length > this.buffer.Length)
            {
                throw new RpcException("Buffer overflow.");
            }

            Span<byte> span = this.buffer.AsSpan(this.index, length);
            this.index += length;
            return span;
        }
    }
}
