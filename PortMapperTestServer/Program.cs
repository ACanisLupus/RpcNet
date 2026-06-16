// Copyright by Artur Wolf

using System.Net;
using RpcNet;
using RpcNet.PortMapper;
using Test;

CancellationToken ct = CancellationToken.None;

if ((args.Length != 1) || !IPEndPoint.TryParse(args[0], out IPEndPoint? ipEndPoint))
{
    ipEndPoint = new IPEndPoint(IPAddress.IPv6Any, 111);
}

await using PortMapperServer portMapperServer = new(
    Protocol.Tcp | Protocol.Udp,
    ipEndPoint.Address,
    ipEndPoint.Port,
    new ServerSettings
    {
        Logger = new TestLogger("Port Mapper")
    });
await portMapperServer.StartAsync(ct).ConfigureAwait(false);

await Task.Delay(-1).ConfigureAwait(false);
