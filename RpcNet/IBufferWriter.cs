namespace RpcNet
{
    using System;

    public interface IBufferWriter
    {
        void BeginWriting();
        void EndWriting();
        Span<byte> Reserve(int length);
    }
}
