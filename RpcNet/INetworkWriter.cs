namespace RpcNet
{
    using System;

    public interface INetworkWriter
    {
        void Reset();
        void EndWriting();
        Span<byte> Reserve(int length);
    }
}
