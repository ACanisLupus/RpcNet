// Copyright by Artur Wolf

namespace RpcNet;

using System.Net;

public sealed class RpcEndPoint(EndPoint endPoint, Protocol protocol)
{
    public EndPoint EndPoint { get; } = endPoint;
    public Protocol Protocol { get; } = protocol;

    public override string ToString() => $"{Protocol.ToString().ToUpper()}:{EndPoint}";
}
