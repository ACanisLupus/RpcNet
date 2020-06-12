namespace RpcNet.Internal
{
    using System;

    // Public for tests
    public interface INetworkWriter
    {
        Span<byte> Reserve(int length);
    }
}
