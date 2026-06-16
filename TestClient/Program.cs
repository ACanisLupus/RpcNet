// Copyright by Artur Wolf

using System.Net;
using RpcNet;
using Test;
using TestService;

CancellationToken ct = CancellationToken.None;

if ((args.Length != 1) || !IPEndPoint.TryParse(args[0], out IPEndPoint? ipEndPoint))
{
    ipEndPoint = new IPEndPoint(IPAddress.IPv6Any, 0);
}

TestLogger logger = new("Test Client");
using (TestServiceClient testTcpClient = await TestServiceClient.ConnectAsync(
               Protocol.Tcp,
               ipEndPoint.Address,
               ipEndPoint.Port,
               new ClientSettings
               {
                   Logger = logger
               },
               ct)
           .ConfigureAwait(false))
{
    for (int i = 0; i < 2; i++)
    {
        _ = await testTcpClient.Echo_1Async(i, ct);
    }
}

using TestServiceClient testUdpClient = await TestServiceClient.ConnectAsync(
        Protocol.Udp,
        ipEndPoint.Address,
        ipEndPoint.Port,
        new ClientSettings
        {
            Logger = logger
        },
        ct)
    .ConfigureAwait(false);
for (int i = 0; i < 2; i++)
{
    _ = await testUdpClient.Echo_1Async(i, ct);
}
