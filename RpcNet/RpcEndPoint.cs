// Copyright by Artur Wolf

namespace RpcNet;

using System.Net;
using RpcNet.Internal;

public sealed class RpcEndPoint
{
    public RpcEndPoint(EndPoint endPoint, Protocol protocol)
    {
        EndPoint = endPoint;
        Protocol = protocol;
    }

    public EndPoint EndPoint { get; }
    public Protocol Protocol { get; }

    public override string ToString() => $"{Protocol.ToString().ToUpper()}:{EndPoint}";
}
