namespace RpcNet
{
    using System;

    public interface INetworkReader
    {
        ReadOnlySpan<byte> Read(int length);
    }
}
