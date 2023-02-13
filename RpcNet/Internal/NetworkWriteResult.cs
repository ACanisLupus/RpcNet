// Copyright by Artur Wolf

namespace RpcNet.Internal;

using System.Net.Sockets;

// Public for tests
public readonly struct NetworkWriteResult
{
    public NetworkWriteResult(SocketError socketError) => SocketError = socketError;

    public SocketError SocketError { get; }
    public bool HasError => SocketError != SocketError.Success;
}
