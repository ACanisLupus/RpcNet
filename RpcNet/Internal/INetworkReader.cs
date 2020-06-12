namespace RpcNet.Internal
{
    using System;

    // Public for tests
    public interface INetworkReader
    {
        ReadOnlySpan<byte> Read(int length);
    }
}
