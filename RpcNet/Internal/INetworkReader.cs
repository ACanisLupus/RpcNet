namespace RpcNet.Internal
{
    using System;

    // Public for tests
    public interface INetworkReader
    {
        NetworkReadResult BeginReading();
        void EndReading();
        ReadOnlySpan<byte> Read(int length);
    }
}
