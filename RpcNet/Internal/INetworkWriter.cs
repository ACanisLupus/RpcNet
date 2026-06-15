// Copyright by Artur Wolf

namespace RpcNet.Internal;

using System.Net;

internal interface INetworkWriter
{
    void BeginWriting();
    ValueTask EndWritingAsync(EndPoint remoteEndPoint, CancellationToken cancellationToken);
    Span<byte> Reserve(int length);
}
