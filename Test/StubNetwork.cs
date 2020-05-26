namespace RpcNet.Test
{
    using System;
    using RpcNet;

    internal class StubNetwork : INetworkReader, INetworkWriter
    {
        private readonly byte[] buffer = new byte[65536];

        public int ReadIndex { get; private set; }
        public int WriteIndex { get; private set; }

        public void Reset()
        {
            this.ReadIndex = 0;
            this.WriteIndex = 0;
        }

        public void BeginReading() => throw new NotImplementedException();
        public void BeginWriting() => throw new NotImplementedException();
        public void EndReading() => throw new NotImplementedException();
        public void EndWriting() => throw new NotImplementedException();

        public Span<byte> Read(int length)
        {
            Span<byte> span = this.buffer.AsSpan(this.ReadIndex, length);
            this.ReadIndex += length;
            return span;
        }

        public Span<byte> Reserve(int length)
        {
            Span<byte> span = this.buffer.AsSpan(this.WriteIndex, length);
            this.WriteIndex += length;
            return span;
        }
    }
}
