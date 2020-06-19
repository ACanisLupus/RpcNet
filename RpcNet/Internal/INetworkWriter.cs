namespace RpcNet.Internal
{
    using System;

    public interface INetworkWriter
    {
        void BeginWriting();
        NetworkResult EndWriting();
        Span<byte> Reserve(int length);
    }
}
