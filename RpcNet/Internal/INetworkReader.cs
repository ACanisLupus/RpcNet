// Copyright by Artur Wolf

namespace RpcNet.Internal;

using System.Net;

internal interface INetworkReader
{
    ValueTask<EndPoint> BeginReadingAsync(CancellationToken cancellationToken);
    void EndReading();
    ReadOnlySpan<byte> Read(int length);
}
