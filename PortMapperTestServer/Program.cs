// Copyright by Artur Wolf

using System.Net;
using RpcNet;
using RpcNet.PortMapper;
using Test;

if ((args.Length != 1) || !IPEndPoint.TryParse(args[0], out IPEndPoint? ipEndPoint))
{
    ipEndPoint = new IPEndPoint(IPAddress.IPv6Any, 111);
}

using var portMapperServer = new PortMapperServer(
    Protocol.TcpAndUdp,
    ipEndPoint.Address,
    ipEndPoint.Port,
    new ServerSettings { Logger = new TestLogger("Port Mapper") });
portMapperServer.Start();

Thread.Sleep(-1);
