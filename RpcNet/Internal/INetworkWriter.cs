namespace RpcNet.Internal
{
    using System;

    public interface INetworkWriter
    {
        void BeginWriting();
        NetworkWriteResult EndWriting();
        Span<byte> Reserve(int length);
    }
}
