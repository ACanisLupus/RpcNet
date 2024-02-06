// Copyright by Artur Wolf

namespace RpcNet.Internal;

using System.Net;
using System.Net.Sockets;

// Public for tests
public readonly struct NetworkReadResult
{
    private NetworkReadResult(IPEndPoint? remoteIpEndPoint, SocketError socketError, bool isDisconnected)
    {
        RemoteIpEndPoint = remoteIpEndPoint;
        SocketError = socketError;
        IsDisconnected = isDisconnected;
    }

    public IPEndPoint? RemoteIpEndPoint { get; }
    public SocketError SocketError { get; }
    public bool IsDisconnected { get; }
    public bool HasError => SocketError != SocketError.Success;

    public static NetworkReadResult CreateError(SocketError socketError) => new(null, socketError, false);
    public static NetworkReadResult CreateDisconnected() => new(null, SocketError.Success, true);
    public static NetworkReadResult CreateSuccess(IPEndPoint? remoteIpEndPoint = default) => new(remoteIpEndPoint, SocketError.Success, false);
}
