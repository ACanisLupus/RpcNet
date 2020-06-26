namespace RpcNet.Test
{
    using System;
    using RpcNet.Internal;

    internal class StubNetwork : INetworkReader, INetworkWriter
    {
        private readonly byte[] buffer = new byte[65536];
        private readonly int maxReadLength;
        private readonly int maxReserveLength;

        public StubNetwork(int maxReadLength, int maxReserveLength)
        {
            this.maxReadLength = maxReadLength;
            this.maxReserveLength = maxReserveLength;
        }

        public int ReadIndex { get; private set; }
        public int WriteIndex { get; private set; }

        public NetworkReadResult BeginReading() => new NetworkReadResult();

        public void EndReading()
        {
        }

        public void BeginWriting()
        {
        }

        public NetworkWriteResult EndWriting() => new NetworkWriteResult();

        public void Reset()
        {
            this.ReadIndex = 0;
            this.WriteIndex = 0;
        }

        public ReadOnlySpan<byte> Read(int length)
        {
            length = Math.Min(length, this.maxReadLength);
            Span<byte> span = this.buffer.AsSpan(this.ReadIndex, length);
            this.ReadIndex += length;
            return span;
        }

        public Span<byte> Reserve(int length)
        {
            length = Math.Min(length, this.maxReserveLength);
            Span<byte> span = this.buffer.AsSpan(this.WriteIndex, length);
            this.WriteIndex += length;
            return span;
        }
    }
}
