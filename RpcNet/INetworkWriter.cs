namespace RpcNet
{
    using System;

    public interface INetworkWriter
    {
        Span<byte> Reserve(int length);
    }
}
