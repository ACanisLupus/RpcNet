// Copyright by Artur Wolf

namespace Test;

using System.Net;
using System.Net.Sockets;
using RpcNet;

internal static class Utilities
{
    public static IEnumerable<IPAddress> GetIpAddresses()
    {
        if (Socket.OSSupportsIPv4)
        {
            yield return IPAddress.Loopback;
        }

        if (Socket.OSSupportsIPv6)
        {
            yield return IPAddress.IPv6Loopback;
        }
    }

    public static IEnumerable<object[]> GetProtocolAndAddressCases()
    {
        if (Socket.OSSupportsIPv4)
        {
            yield return [Protocol.Tcp, IPAddress.Loopback];
            yield return [Protocol.Udp, IPAddress.Loopback];
        }

        if (Socket.OSSupportsIPv6)
        {
            yield return [Protocol.Tcp, IPAddress.IPv6Loopback];
            yield return [Protocol.Udp, IPAddress.IPv6Loopback];
        }
    }

    public static int GetPort(ServerStub server, Protocol protocol) => protocol == Protocol.Tcp ? server.TcpPort : server.UdpPort;
}
