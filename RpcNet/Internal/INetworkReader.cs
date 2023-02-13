// Copyright by Artur Wolf

namespace RpcNet.Internal;

// Public for tests
public interface INetworkReader
{
    NetworkReadResult BeginReading();
    void EndReading();
    ReadOnlySpan<byte> Read(int length);
}
