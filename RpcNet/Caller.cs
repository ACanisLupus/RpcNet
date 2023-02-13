// Copyright by Artur Wolf

namespace RpcNet;

using System.Net;
using Internal;

public class Caller
{
    public Caller(IPEndPoint ipEndPoint, Protocol protocol)
    {
        IpEndPoint = ipEndPoint;
        Protocol = protocol;
    }

    public IPEndPoint IpEndPoint { get; }
    public Protocol Protocol { get; }

    public override string ToString() => $"{Utilities.ConvertToString(Protocol)}:{IpEndPoint}";
}
