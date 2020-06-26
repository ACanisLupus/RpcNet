namespace RpcNet.Internal
{
    using System;

    public interface INetworkReader
    {
        NetworkReadResult BeginReading();
        void EndReading();
        ReadOnlySpan<byte> Read(int length);
    }
}
