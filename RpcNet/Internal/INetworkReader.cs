namespace RpcNet.Internal
{
    using System;

    public interface INetworkReader
    {
        NetworkResult BeginReading();
        void EndReading();
        ReadOnlySpan<byte> Read(int length);
    }
}
