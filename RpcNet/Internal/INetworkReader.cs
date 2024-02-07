// Copyright by Artur Wolf

namespace RpcNet.Internal;

using System.Net;

// Public for tests
public interface INetworkReader
{
    IPEndPoint BeginReading();
    void EndReading();
    ReadOnlySpan<byte> Read(int length);
}
