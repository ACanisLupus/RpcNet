namespace RpcNet
{
    using System;

    public interface INetworkReader
    {
        Span<byte> Read(int length);
    }
}
