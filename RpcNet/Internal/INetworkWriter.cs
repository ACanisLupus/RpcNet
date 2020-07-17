namespace RpcNet.Internal
{
    using System;
    using System.Net;

    public interface INetworkWriter
    {
        void BeginWriting();
        NetworkWriteResult EndWriting(IPEndPoint remoteEndPoint);
        Span<byte> Reserve(int length);
    }
}
