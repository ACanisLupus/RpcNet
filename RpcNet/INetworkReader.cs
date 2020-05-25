namespace RpcNet
{
    using System;

    public interface INetworkReader
    {
        void BeginReading();
        void EndReading();
        Span<byte> Read(int length);
    }
}
