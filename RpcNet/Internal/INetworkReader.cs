// Copyright by Artur Wolf

namespace RpcNet.Internal;

using System.Net;

internal interface INetworkReader
{
    EndPoint BeginReading();
    void EndReading();
    ReadOnlySpan<byte> Read(int length);
}
