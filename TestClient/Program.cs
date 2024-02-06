// Copyright by Artur Wolf

using System.Net;
using RpcNet;
using Test;
using TestService;

if ((args.Length != 1) || !IPEndPoint.TryParse(args[0], out IPEndPoint? ipEndPoint))
{
    ipEndPoint = new IPEndPoint(IPAddress.IPv6Any, 0);
}

var logger = new TestLogger("Test Client");
using (var testTcpClient = new TestServiceClient(Protocol.Tcp, ipEndPoint.Address, ipEndPoint.Port, new ClientSettings { Logger = logger }))
{
    for (int i = 0; i < 2; i++)
    {
        _ = testTcpClient.Echo_1(i);
    }
}

using var testUdpClient = new TestServiceClient(Protocol.Udp, ipEndPoint.Address, ipEndPoint.Port, new ClientSettings { Logger = logger });
for (int i = 0; i < 2; i++)
{
    _ = testUdpClient.Echo_1(i);
}
