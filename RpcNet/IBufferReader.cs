namespace RpcNet
{
    using System;

    public interface IBufferReader
    {
        void BeginReading();
        Span<byte> Read(int length);
    }
}
