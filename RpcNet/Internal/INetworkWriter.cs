// Copyright by Artur Wolf

namespace RpcNet.Internal;

using System.Net;

// Public for tests
public interface INetworkWriter
{
    void BeginWriting();
    void EndWriting(IPEndPoint remoteEndPoint);
    Span<byte> Reserve(int length);
}
